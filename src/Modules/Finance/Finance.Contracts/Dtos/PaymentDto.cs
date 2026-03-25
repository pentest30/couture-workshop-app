namespace Couture.Finance.Contracts.Dtos;

public sealed record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    string PaymentMethodLabel,
    DateOnly PaymentDate,
    string? Note,
    string? ReceiptCode,
    Guid RecordedBy,
    DateTimeOffset CreatedAt);
