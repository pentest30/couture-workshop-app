using Couture.SharedKernel;

namespace Couture.Orders.Contracts.Events;

public sealed record StatusChangedEvent(
    OrderId OrderId,
    string OrderCode,
    string FromStatus,
    string ToStatus,
    string? Reason,
    Guid ChangedByUserId,
    Guid? AssignedEmbroidererId,
    Guid? AssignedBeaderId) : IDomainEvent;
