using Couture.Orders.Contracts;

namespace Couture.Orders.Domain;

public sealed class StatusTransition
{
    public StatusTransitionId Id { get; private set; }
    public OrderId OrderId { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; } = default!;
    public string? Reason { get; private set; }
    public string TransitionedBy { get; private set; } = default!;
    public DateTimeOffset TransitionedAt { get; private set; }

    private StatusTransition() { }

    internal static StatusTransition Create(OrderId orderId, OrderStatus? from, OrderStatus to, string? reason, string transitionedBy)
    {
        return new StatusTransition
        {
            Id = StatusTransitionId.From(Guid.NewGuid()),
            OrderId = orderId,
            FromStatus = from,
            ToStatus = to,
            Reason = reason,
            TransitionedBy = transitionedBy,
            TransitionedAt = DateTimeOffset.UtcNow,
        };
    }
}
