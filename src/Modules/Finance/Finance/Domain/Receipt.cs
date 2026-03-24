using Couture.Finance.Contracts;

namespace Couture.Finance.Domain;

public sealed class Receipt
{
    public ReceiptId Id { get; private set; }
    public string Code { get; private set; } = default!;
    public PaymentId PaymentId { get; private set; }
    public string PdfStoragePath { get; private set; } = default!;
    public DateTimeOffset GeneratedAt { get; private set; }

    private Receipt() { }

    public static Receipt Create(string code, PaymentId paymentId, string pdfStoragePath)
    {
        return new Receipt
        {
            Id = ReceiptId.From(Guid.NewGuid()),
            Code = code,
            PaymentId = paymentId,
            PdfStoragePath = pdfStoragePath,
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }
}
