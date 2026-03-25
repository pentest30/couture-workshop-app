namespace Couture.Catalog.Contracts;
public readonly record struct ModelId(Guid Value)
{
    public static ModelId From(Guid value) => new(value);
}
