namespace Couture.Clients.Contracts;
public readonly record struct MeasurementFieldId(Guid Value)
{
    public static MeasurementFieldId From(Guid value) => new(value);
    public static MeasurementFieldId NewMeasurementFieldId() => new(Guid.NewGuid());
}
