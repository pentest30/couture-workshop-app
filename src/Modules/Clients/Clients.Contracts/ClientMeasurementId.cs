namespace Couture.Clients.Contracts;
public readonly record struct ClientMeasurementId(Guid Value)
{
    public static ClientMeasurementId From(Guid value) => new(value);
    public static ClientMeasurementId NewClientMeasurementId() => new(Guid.NewGuid());
}
