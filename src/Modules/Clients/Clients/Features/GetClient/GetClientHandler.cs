using Couture.Clients.Contracts;
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
        var id = ClientId.From(query.ClientId);
        var client = await _db.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (client is null) return null;

        // Get all measurements for this client, then group in memory
        var allMeasurements = await _db.ClientMeasurements
            .AsNoTracking()
            .Where(m => m.ClientId == id)
            .ToListAsync(ct);

        var fields = await _db.MeasurementFields
            .AsNoTracking()
            .Where(f => f.IsActive)
            .ToDictionaryAsync(f => f.Id, f => f, ct);

        // Get latest measurement per field
        var measurements = allMeasurements
            .GroupBy(m => m.MeasurementFieldId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(m => m.MeasuredAt).First();
                var field = fields.GetValueOrDefault(latest.MeasurementFieldId);
                return new MeasurementDto(
                    latest.MeasurementFieldId.Value,
                    field?.Name ?? "Inconnu",
                    field?.Unit ?? "cm",
                    latest.Value,
                    latest.MeasuredAt);
            })
            .ToList();

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
            new ClientStatsDto(0, 0, null, 0),
            client.CreatedAt);
    }
}
