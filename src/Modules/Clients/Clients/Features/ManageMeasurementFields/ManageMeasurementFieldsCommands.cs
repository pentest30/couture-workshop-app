using Mediator;
namespace Couture.Clients.Features.ManageMeasurementFields;
public sealed record CreateMeasurementFieldCommand(string Name, string Unit, int DisplayOrder) : ICommand<Guid>;
public sealed record DeleteMeasurementFieldCommand(Guid FieldId) : ICommand;
