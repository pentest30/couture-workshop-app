using Xunit;
using Couture.Finance.Features.RecordPayment;
using Couture.Orders.Domain;
using FluentAssertions;

namespace Couture.Finance.Tests;

public class RecordPaymentHandlerTests
{
    private static Guid SeedOrder(Couture.Orders.Persistence.OrdersDbContext db, decimal totalPrice = 20000m)
    {
        var order = Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), totalPrice);
        db.Orders.Add(order);
        db.SaveChanges();
        return order.Id.Value;
    }

    [Fact]
    public async Task Handle_ValidPayment_RecordsAndGeneratesReceipt()
    {
        var (financeDb, ordersDb) = TestDbHelper.CreateInMemoryContexts();
        var orderId = SeedOrder(ordersDb);
        var handler = new RecordPaymentHandler(financeDb, ordersDb);

        var result = await handler.Handle(
            new RecordPaymentCommand(orderId, 5000m, "Especes",
                DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid()),
            CancellationToken.None);

        result.NewOutstandingBalance.Should().Be(15000m);
        result.ReceiptCode.Should().StartWith("REC-");
        financeDb.Payments.Should().HaveCount(1);
        financeDb.Receipts.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_MultiplePayments_CalculatesBalanceCorrectly()
    {
        var (financeDb, ordersDb) = TestDbHelper.CreateInMemoryContexts();
        var orderId = SeedOrder(ordersDb, 20000m);
        var handler = new RecordPaymentHandler(financeDb, ordersDb);
        var userId = Guid.NewGuid();

        var r1 = await handler.Handle(
            new RecordPaymentCommand(orderId, 5000m, "Especes", DateOnly.FromDateTime(DateTime.UtcNow), null, userId),
            CancellationToken.None);
        r1.NewOutstandingBalance.Should().Be(15000m);

        var r2 = await handler.Handle(
            new RecordPaymentCommand(orderId, 10000m, "BaridiMob", DateOnly.FromDateTime(DateTime.UtcNow), null, userId),
            CancellationToken.None);
        r2.NewOutstandingBalance.Should().Be(5000m);

        financeDb.Payments.Should().HaveCount(2);
        financeDb.Receipts.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ExceedsBalance_Throws()
    {
        var (financeDb, ordersDb) = TestDbHelper.CreateInMemoryContexts();
        var orderId = SeedOrder(ordersDb, 10000m);
        var handler = new RecordPaymentHandler(financeDb, ordersDb);

        var act = async () => await handler.Handle(
            new RecordPaymentCommand(orderId, 15000m, "Especes",
                DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*exceeds*");
    }

    [Fact]
    public async Task Handle_OrderNotFound_Throws()
    {
        var (financeDb, ordersDb) = TestDbHelper.CreateInMemoryContexts();
        var handler = new RecordPaymentHandler(financeDb, ordersDb);

        var act = async () => await handler.Handle(
            new RecordPaymentCommand(Guid.NewGuid(), 5000m, "Especes",
                DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ReceiptCodes_AreSequential()
    {
        var (financeDb, ordersDb) = TestDbHelper.CreateInMemoryContexts();
        var orderId = SeedOrder(ordersDb, 30000m);
        var handler = new RecordPaymentHandler(financeDb, ordersDb);

        var r1 = await handler.Handle(
            new RecordPaymentCommand(orderId, 5000m, "Especes", DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid()),
            CancellationToken.None);
        var r2 = await handler.Handle(
            new RecordPaymentCommand(orderId, 5000m, "Virement", DateOnly.FromDateTime(DateTime.UtcNow), null, Guid.NewGuid()),
            CancellationToken.None);

        r1.ReceiptCode.Should().EndWith("0001");
        r2.ReceiptCode.Should().EndWith("0002");
    }
}
