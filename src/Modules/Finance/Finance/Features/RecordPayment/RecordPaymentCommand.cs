using Mediator;

namespace Couture.Finance.Features.RecordPayment;

public sealed record RecordPaymentCommand(
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    DateOnly PaymentDate,
    string? Note,
    Guid RecordedBy) : ICommand<RecordPaymentResult>;

public sealed record RecordPaymentResult(
    Guid PaymentId,
    string? ReceiptCode,
    decimal NewOutstandingBalance);
