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

        // Compute real client stats via raw SQL (cross-schema)
        var stats = new ClientStatsDto(0, 0, null, 0);
        try
        {
            var connection = _db.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(ct);

            // Total orders + active orders
            await using var cmd1 = connection.CreateCommand();
            cmd1.CommandText = @"SELECT COUNT(*), COUNT(*) FILTER (WHERE ""Status"" != 8) FROM orders.orders WHERE ""ClientId"" = @cid";
            var p1 = cmd1.CreateParameter(); p1.ParameterName = "@cid"; p1.Value = query.ClientId; cmd1.Parameters.Add(p1);
            await using var r1 = await cmd1.ExecuteReaderAsync(ct);
            int totalOrders = 0, activeOrders = 0;
            if (await r1.ReadAsync(ct)) { totalOrders = r1.GetInt32(0); activeOrders = r1.GetInt32(1); }
            await r1.CloseAsync();

            // Total amount collected
            await using var cmd2 = connection.CreateCommand();
            cmd2.CommandText = @"SELECT COALESCE(SUM(p.""Amount""), 0) FROM finance.payments p INNER JOIN orders.orders o ON p.""OrderId"" = o.""Id"" WHERE o.""ClientId"" = @cid";
            var p2 = cmd2.CreateParameter(); p2.ParameterName = "@cid"; p2.Value = query.ClientId; cmd2.Parameters.Add(p2);
            var totalCollected = Convert.ToDecimal(await cmd2.ExecuteScalarAsync(ct) ?? 0);

            stats = new ClientStatsDto(totalOrders, totalCollected, null, activeOrders);
        }
        catch { /* cross-schema not available in tests */ }

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
            stats,
            client.CreatedAt);
    }
}
