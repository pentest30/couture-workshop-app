using Couture.Clients.Contracts;

namespace Couture.Clients.Domain;

public sealed class MeasurementField
{
    public MeasurementFieldId Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Unit { get; private set; } = "cm";
    public int DisplayOrder { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;

    private MeasurementField() { }

    public static MeasurementField Create(string name, string unit, int displayOrder, bool isDefault = false)
    {
        return new MeasurementField
        {
            Id = MeasurementFieldId.From(Guid.NewGuid()),
            Name = name,
            Unit = unit,
            DisplayOrder = displayOrder,
            IsDefault = isDefault,
            IsActive = true,
        };
    }

    public void Deactivate() => IsActive = false;
    public void UpdateOrder(int order) => DisplayOrder = order;
}
