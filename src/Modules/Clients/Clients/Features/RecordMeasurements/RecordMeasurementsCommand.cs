using Mediator;
namespace Couture.Clients.Features.RecordMeasurements;
public sealed record RecordMeasurementsCommand(Guid ClientId, List<MeasurementEntry> Measurements, Guid MeasuredBy) : ICommand;
public sealed record MeasurementEntry(Guid MeasurementFieldId, decimal Value);
