using Couture.Clients.Contracts.Dtos;
using Couture.Clients.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Clients.Features.GetClient;

public sealed class GetClientHandler : IQueryHandler<GetClientQuery, ClientDetailDto?>
{
    private readonly ClientsDbContext _db;

    public GetClientHandler(ClientsDbContext db) => _db = db;

    public async ValueTask<ClientDetailDto?> Handle(GetClientQuery query, CancellationToken ct)
    {
        var client = await _db.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id.Value == query.ClientId, ct);

        if (client is null) return null;

        // Get current measurements (latest per field)
        var measurements = await _db.ClientMeasurements
            .AsNoTracking()
            .Where(m => m.ClientId.Value == query.ClientId)
            .Join(_db.MeasurementFields, m => m.MeasurementFieldId, f => f.Id,
                (m, f) => new { m, f })
            .GroupBy(x => x.m.MeasurementFieldId)
            .Select(g => g.OrderByDescending(x => x.m.MeasuredAt).First())
            .Select(x => new MeasurementDto(
                x.f.Id.Value,
                x.f.Name,
                x.f.Unit,
                x.m.Value,
                x.m.MeasuredAt))
            .ToListAsync(ct);

        return new ClientDetailDto(
            client.Id.Value,
            client.Code,
            client.FirstName,
            client.LastName,
            client.FullName,
            client.PrimaryPhone,
            client.SecondaryPhone,
            client.Address,
            client.DateOfBirth,
            client.Notes,
            measurements,
            new ClientStatsDto(0, 0, null, 0), // Cross-module stats deferred
            client.CreatedAt);
    }
}
