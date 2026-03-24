using Couture.Clients.Contracts;

namespace Couture.Clients.Domain;

public sealed class ClientMeasurement
{
    public ClientMeasurementId Id { get; private set; }
    public ClientId ClientId { get; private set; }
    public MeasurementFieldId MeasurementFieldId { get; private set; }
    public decimal Value { get; private set; }
    public DateTimeOffset MeasuredAt { get; private set; }
    public Guid MeasuredBy { get; private set; }

    private ClientMeasurement() { }

    public static ClientMeasurement Create(ClientId clientId, MeasurementFieldId fieldId, decimal value, Guid measuredBy)
    {
        if (value <= 0) throw new ArgumentException("Measurement value must be positive.");
        return new ClientMeasurement
        {
            Id = ClientMeasurementId.From(Guid.NewGuid()),
            ClientId = clientId,
            MeasurementFieldId = fieldId,
            Value = value,
            MeasuredAt = DateTimeOffset.UtcNow,
            MeasuredBy = measuredBy,
        };
    }
}
