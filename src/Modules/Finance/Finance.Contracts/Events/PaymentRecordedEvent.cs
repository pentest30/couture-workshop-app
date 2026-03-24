using Couture.SharedKernel;
namespace Couture.Finance.Contracts.Events;
public sealed record PaymentRecordedEvent(
    PaymentId PaymentId,
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    decimal NewOutstandingBalance) : IDomainEvent;
