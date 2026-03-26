using Mediator;

namespace Couture.Orders.Features.ChangeStatus;

public sealed record ChangeStatusCommand(
    Guid OrderId,
    string NewStatus,
    string? Reason,
    Guid? AssignedTailorId,
    Guid? AssignedEmbroidererId,
    Guid? AssignedBeaderId,
    DateOnly? ActualDeliveryDate,
    Guid ChangedByUserId) : ICommand<ChangeStatusResult>;

public sealed record ChangeStatusResult(Guid OrderId, string PreviousStatus, string NewStatus, DateTimeOffset TransitionedAt);
