using Couture.Clients.Domain;
using Couture.Clients.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Clients.Features.CreateClient;

public sealed class CreateClientHandler : ICommandHandler<CreateClientCommand, CreateClientResult>
{
    private readonly ClientsDbContext _db;

    public CreateClientHandler(ClientsDbContext db) => _db = db;

    public async ValueTask<CreateClientResult> Handle(CreateClientCommand command, CancellationToken ct)
    {
        // Check duplicate phone
        var existingClient = await _db.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PrimaryPhone == command.PrimaryPhone.Trim(), ct);

        if (existingClient is not null)
            throw new InvalidOperationException($"A client with phone {command.PrimaryPhone} already exists: {existingClient.Code} - {existingClient.FullName}");

        // Generate sequential code
        var count = await _db.Clients.CountAsync(ct) + 1;
        var code = $"C-{count:D4}";

        var client = Client.Create(
            code, command.FirstName, command.LastName, command.PrimaryPhone,
            command.SecondaryPhone, command.Address, command.DateOfBirth, command.Notes);

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        return new CreateClientResult(client.Id.Value, client.Code);
    }
}
