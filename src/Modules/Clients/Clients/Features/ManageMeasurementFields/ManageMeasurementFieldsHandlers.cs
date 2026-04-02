using Couture.Clients.Contracts;
using Couture.Clients.Domain;
using Couture.Clients.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace Couture.Clients.Features.ManageMeasurementFields;
public sealed class CreateMeasurementFieldHandler : ICommandHandler<CreateMeasurementFieldCommand, Guid>
{
    private readonly ClientsDbContext _db;
    public CreateMeasurementFieldHandler(ClientsDbContext db) => _db = db;
    public async ValueTask<Guid> Handle(CreateMeasurementFieldCommand cmd, CancellationToken ct)
    {
        var existing = await _db.MeasurementFields
            .FirstOrDefaultAsync(f => f.Name == cmd.Name, ct);

        if (existing is not null)
            throw new InvalidOperationException($"A measurement field named '{cmd.Name}' already exists.");

        var field = MeasurementField.Create(cmd.Name, cmd.Unit, cmd.DisplayOrder);
        _db.MeasurementFields.Add(field);
        await _db.SaveChangesAsync(ct);
        return field.Id.Value;
    }
}
public sealed class DeleteMeasurementFieldHandler : ICommandHandler<DeleteMeasurementFieldCommand>
{
    private readonly ClientsDbContext _db;
    public DeleteMeasurementFieldHandler(ClientsDbContext db) => _db = db;
    public async ValueTask<Unit> Handle(DeleteMeasurementFieldCommand cmd, CancellationToken ct)
    {
        var id = MeasurementFieldId.From(cmd.FieldId);
        var field = await _db.MeasurementFields.FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new InvalidOperationException("Measurement field not found.");
        field.Deactivate();
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
