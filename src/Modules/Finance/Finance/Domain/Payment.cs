using Couture.Finance.Contracts;
using Couture.SharedKernel;

namespace Couture.Finance.Domain;

public sealed class Payment : AuditableEntity
{
    public PaymentId Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; } = default!;
    public DateOnly PaymentDate { get; private set; }
    public string? Note { get; private set; }
    public Guid RecordedBy { get; private set; }
    public Receipt? Receipt { get; private set; }

    private Payment() { }

    public static Payment Create(Guid orderId, decimal amount, PaymentMethod method, DateOnly paymentDate, Guid recordedBy, string? note = null)
    {
        if (amount <= 0) throw new ArgumentException("Payment amount must be positive.");
        return new Payment
        {
            Id = PaymentId.From(Guid.NewGuid()),
            OrderId = orderId,
            Amount = amount,
            PaymentMethod = method,
            PaymentDate = paymentDate,
            RecordedBy = recordedBy,
            Note = note?.Trim(),
        };
    }

    public void AttachReceipt(Receipt receipt) => Receipt = receipt;
}
