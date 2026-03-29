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
        // Check duplicate phone — block unless caller explicitly confirms
        var existingClient = await _db.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PrimaryPhone == command.PrimaryPhone.Trim(), ct);

        if (existingClient is not null && !command.ConfirmDuplicate)
            throw new DuplicatePhoneException(existingClient.Id.Value, existingClient.Code, existingClient.FullName, command.PrimaryPhone.Trim());

        // Generate sequential code (include soft-deleted clients to avoid duplicate codes)
        var lastCode = await _db.Clients
            .IgnoreQueryFilters()
            .OrderByDescending(c => c.Code)
            .Select(c => c.Code)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastCode is not null && lastCode.StartsWith("C-") && int.TryParse(lastCode[2..], out var parsed))
            nextNumber = parsed + 1;

        var code = $"C-{nextNumber:D4}";

        var client = Client.Create(
            code, command.FirstName, command.LastName, command.PrimaryPhone,
            command.SecondaryPhone, command.Address, command.DateOfBirth, command.Notes);

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        return new CreateClientResult(client.Id.Value, client.Code);
    }
}

public sealed class DuplicatePhoneException(Guid existingClientId, string existingCode, string existingName, string phone) : InvalidOperationException(
    $"Un client avec le téléphone {phone} existe déjà : {existingCode} — {existingName}")
{
    public Guid ExistingClientId { get; } = existingClientId;
    public string ExistingCode { get; } = existingCode;
    public string ExistingName { get; } = existingName;
}
