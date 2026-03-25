using Couture.Finance.Domain;
using Couture.Finance.Persistence;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Finance.Features.RecordPayment;

public sealed class RecordPaymentHandler : ICommandHandler<RecordPaymentCommand, RecordPaymentResult>
{
    private readonly FinanceDbContext _financeDb;
    private readonly OrdersDbContext _ordersDb;

    public RecordPaymentHandler(FinanceDbContext financeDb, OrdersDbContext ordersDb)
    {
        _financeDb = financeDb;
        _ordersDb = ordersDb;
    }

    public async ValueTask<RecordPaymentResult> Handle(RecordPaymentCommand command, CancellationToken ct)
    {
        // Get order to validate
        var orderId = Couture.Orders.Contracts.OrderId.From(command.OrderId);
        var order = await _ordersDb.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new InvalidOperationException("Order not found.");

        // Calculate current outstanding balance
        var totalPaid = await _financeDb.Payments
            .Where(p => p.OrderId == command.OrderId)
            .SumAsync(p => p.Amount, ct);

        var outstandingBefore = order.TotalPrice - totalPaid;

        if (command.Amount > outstandingBefore)
            throw new InvalidOperationException(
                $"Payment amount ({command.Amount} DZD) exceeds outstanding balance ({outstandingBefore} DZD).");

        var method = PaymentMethod.FromName(command.PaymentMethod, ignoreCase: true);

        var payment = Payment.Create(
            command.OrderId, command.Amount, method,
            command.PaymentDate, command.RecordedBy, command.Note);

        // Generate receipt
        var year = DateTime.UtcNow.Year;
        var receiptCount = await _financeDb.Receipts.CountAsync(r => r.Code.StartsWith($"REC-{year}"), ct) + 1;
        var receiptCode = $"REC-{year}-{receiptCount:D4}";

        var receipt = Receipt.Create(receiptCode, payment.Id, $"receipts/{receiptCode}.pdf");
        payment.AttachReceipt(receipt);

        _financeDb.Payments.Add(payment);
        await _financeDb.SaveChangesAsync(ct);

        var newOutstanding = outstandingBefore - command.Amount;

        return new RecordPaymentResult(payment.Id.Value, receiptCode, newOutstanding);
    }
}
