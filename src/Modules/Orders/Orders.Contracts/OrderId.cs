namespace Couture.Orders.Contracts;
public readonly record struct OrderId(Guid Value)
{
    public static OrderId From(Guid value) => new(value);
    public static OrderId NewOrderId() => new(Guid.NewGuid());
}
