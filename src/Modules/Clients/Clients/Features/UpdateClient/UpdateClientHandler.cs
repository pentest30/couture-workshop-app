using Couture.Clients.Contracts;
using Couture.Clients.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace Couture.Clients.Features.UpdateClient;
public sealed class UpdateClientHandler : ICommandHandler<UpdateClientCommand>
{
    private readonly ClientsDbContext _db;
    public UpdateClientHandler(ClientsDbContext db) => _db = db;
    public async ValueTask<Unit> Handle(UpdateClientCommand cmd, CancellationToken ct)
    {
        var id = ClientId.From(cmd.ClientId);
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Client not found.");
        client.Update(cmd.FirstName, cmd.LastName, cmd.PrimaryPhone, cmd.SecondaryPhone, cmd.Address, cmd.DateOfBirth, cmd.Notes);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
