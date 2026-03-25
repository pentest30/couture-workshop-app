using Couture.Clients.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Clients.Features.DeactivateClient;

public sealed record DeactivateClientCommand(Guid ClientId) : ICommand;

public sealed class DeactivateClientHandler : ICommandHandler<DeactivateClientCommand>
{
    private readonly ClientsDbContext _db;
    public DeactivateClientHandler(ClientsDbContext db) => _db = db;

    public async ValueTask<Unit> Handle(DeactivateClientCommand cmd, CancellationToken ct)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == Couture.Clients.Contracts.ClientId.From(cmd.ClientId), ct)
            ?? throw new InvalidOperationException("Client introuvable.");

        // Cross-schema check: any active orders for this client?
        var conn = _db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);
        await using var check = conn.CreateCommand();
        check.CommandText = "SELECT EXISTS(SELECT 1 FROM orders.orders WHERE \"ClientId\" = @cid AND \"IsActive\" = true)";
        var p = check.CreateParameter(); p.ParameterName = "@cid"; p.Value = cmd.ClientId;
        check.Parameters.Add(p);
        var hasOrders = (bool)(await check.ExecuteScalarAsync(ct))!;

        if (hasOrders)
            throw new InvalidOperationException("Impossible de supprimer un client ayant des commandes.");

        client.Deactivate();
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
