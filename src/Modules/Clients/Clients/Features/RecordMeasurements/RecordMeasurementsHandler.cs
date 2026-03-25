using Couture.Clients.Contracts;
using Couture.Clients.Domain;
using Couture.Clients.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace Couture.Clients.Features.RecordMeasurements;
public sealed class RecordMeasurementsHandler : ICommandHandler<RecordMeasurementsCommand>
{
    private readonly ClientsDbContext _db;
    public RecordMeasurementsHandler(ClientsDbContext db) => _db = db;
    public async ValueTask<Unit> Handle(RecordMeasurementsCommand cmd, CancellationToken ct)
    {
        var clientId = ClientId.From(cmd.ClientId);
        var exists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!exists) throw new InvalidOperationException("Client not found.");
        foreach (var m in cmd.Measurements)
        {
            var fieldId = MeasurementFieldId.From(m.MeasurementFieldId);
            _db.ClientMeasurements.Add(ClientMeasurement.Create(clientId, fieldId, m.Value, cmd.MeasuredBy));
        }
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
