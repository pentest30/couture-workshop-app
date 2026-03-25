using Couture.Clients.Contracts.Dtos;
using Couture.Clients.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace Couture.Clients.Features.GetMeasurementHistory;
public sealed class GetMeasurementHistoryHandler : IQueryHandler<GetMeasurementHistoryQuery, MeasurementHistoryResult>
{
    private readonly ClientsDbContext _db;
    public GetMeasurementHistoryHandler(ClientsDbContext db) => _db = db;
    public async ValueTask<MeasurementHistoryResult> Handle(GetMeasurementHistoryQuery query, CancellationToken ct)
    {
        var allMeasurements = await _db.ClientMeasurements.AsNoTracking()
            .Where(m => m.ClientId.Value == query.ClientId)
            .Join(_db.MeasurementFields, m => m.MeasurementFieldId, f => f.Id, (m, f) => new { m, f })
            .OrderByDescending(x => x.m.MeasuredAt).ToListAsync(ct);

        var current = allMeasurements.GroupBy(x => x.m.MeasurementFieldId)
            .Select(g => g.First())
            .Select(x => new MeasurementDto(x.f.Id.Value, x.f.Name, x.f.Unit, x.m.Value, x.m.MeasuredAt))
            .ToList();

        var history = new List<MeasurementHistoryEntryDto>();
        foreach (var group in allMeasurements.GroupBy(x => x.m.MeasurementFieldId))
        {
            var ordered = group.OrderBy(x => x.m.MeasuredAt).ToList();
            for (int i = 1; i < ordered.Count; i++)
                history.Add(new MeasurementHistoryEntryDto(ordered[i].f.Name, ordered[i - 1].m.Value, ordered[i].m.Value, ordered[i].m.MeasuredAt));
        }

        return new MeasurementHistoryResult(current, history.OrderByDescending(h => h.ChangedAt).ToList());
    }
}
