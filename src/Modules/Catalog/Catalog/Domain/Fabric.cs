using Couture.Catalog.Contracts;
using Couture.SharedKernel;

namespace Couture.Catalog.Domain;

public sealed class Fabric : AuditableEntity
{
    public FabricId Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public string Color { get; private set; } = default!;
    public string? Supplier { get; private set; }
    public decimal PricePerMeter { get; private set; }
    public decimal StockMeters { get; private set; }
    public string? Description { get; private set; }
    public string? SwatchPath { get; private set; }

    private Fabric() { }

    public static Fabric Create(string name, string type, string color, decimal pricePerMeter,
        decimal stockMeters = 0, string? supplier = null, string? description = null, string? swatchPath = null)
    {
        return new Fabric
        {
            Id = FabricId.From(Guid.NewGuid()),
            Name = name, Type = type, Color = color,
            PricePerMeter = pricePerMeter, StockMeters = stockMeters,
            Supplier = supplier, Description = description, SwatchPath = swatchPath,
        };
    }

    public void Update(string? name = null, string? type = null, string? color = null,
        decimal? pricePerMeter = null, decimal? stockMeters = null,
        string? supplier = null, string? description = null, string? swatchPath = null)
    {
        if (name is not null) Name = name;
        if (type is not null) Type = type;
        if (color is not null) Color = color;
        if (pricePerMeter.HasValue) PricePerMeter = pricePerMeter.Value;
        if (stockMeters.HasValue) StockMeters = stockMeters.Value;
        if (supplier is not null) Supplier = supplier;
        if (description is not null) Description = description;
        if (swatchPath is not null) SwatchPath = swatchPath;
    }
}
