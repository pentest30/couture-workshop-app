using Ardalis.SmartEnum;

namespace Couture.Orders.Domain;

public sealed class WorkType : SmartEnum<WorkType>
{
    public static readonly WorkType Simple = new(nameof(Simple), 1, "Simple", 1, 3);
    public static readonly WorkType Brode = new(nameof(Brode), 2, "Brodé", 3, 7);
    public static readonly WorkType Perle = new(nameof(Perle), 3, "Perlé", 5, 10);
    public static readonly WorkType Mixte = new(nameof(Mixte), 4, "Mixte", 7, 14);

    public string Label { get; }
    public int MinDeliveryBusinessDays { get; }
    public int StallThresholdDays { get; }

    private WorkType(string name, int value, string label, int minDeliveryBusinessDays, int stallThresholdDays)
        : base(name, value)
    {
        Label = label;
        MinDeliveryBusinessDays = minDeliveryBusinessDays;
        StallThresholdDays = stallThresholdDays;
    }

    public bool RequiresEmbroiderer => this == Brode || this == Mixte;
    public bool RequiresBeader => this == Perle || this == Mixte;
}
