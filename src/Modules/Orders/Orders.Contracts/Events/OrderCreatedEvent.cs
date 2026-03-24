using Couture.SharedKernel;

namespace Couture.Orders.Contracts.Events;

public sealed record OrderCreatedEvent(
    OrderId OrderId,
    string OrderCode,
    string WorkType,
    Guid? AssignedTailorId) : IDomainEvent;
