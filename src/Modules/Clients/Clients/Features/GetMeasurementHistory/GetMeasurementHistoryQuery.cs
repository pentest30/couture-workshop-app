using Couture.Clients.Contracts.Dtos;
using Mediator;
namespace Couture.Clients.Features.GetMeasurementHistory;
public sealed record GetMeasurementHistoryQuery(Guid ClientId) : IQuery<MeasurementHistoryResult>;
public sealed record MeasurementHistoryResult(List<MeasurementDto> Current, List<MeasurementHistoryEntryDto> History);
public sealed record MeasurementHistoryEntryDto(string FieldName, decimal OldValue, decimal NewValue, DateTimeOffset ChangedAt);
