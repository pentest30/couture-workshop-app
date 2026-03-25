namespace Couture.Catalog.Contracts;
public readonly record struct FabricId(Guid Value)
{
    public static FabricId From(Guid value) => new(value);
}
