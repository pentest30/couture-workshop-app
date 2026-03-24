namespace Couture.Orders.Contracts;
public readonly record struct StatusTransitionId(Guid Value)
{
    public static StatusTransitionId From(Guid value) => new(value);
    public static StatusTransitionId NewStatusTransitionId() => new(Guid.NewGuid());
}
