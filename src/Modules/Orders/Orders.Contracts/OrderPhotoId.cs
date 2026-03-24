namespace Couture.Orders.Contracts;
public readonly record struct OrderPhotoId(Guid Value)
{
    public static OrderPhotoId From(Guid value) => new(value);
    public static OrderPhotoId NewOrderPhotoId() => new(Guid.NewGuid());
}
