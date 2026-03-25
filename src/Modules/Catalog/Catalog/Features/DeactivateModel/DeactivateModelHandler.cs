using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.DeactivateModel;

public sealed record DeactivateModelCommand(Guid ModelId) : ICommand;

public sealed class DeactivateModelHandler : ICommandHandler<DeactivateModelCommand>
{
    private readonly CatalogDbContext _db;
    public DeactivateModelHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<Unit> Handle(DeactivateModelCommand cmd, CancellationToken ct)
    {
        var model = await _db.Models.FirstOrDefaultAsync(m => m.Id == Contracts.ModelId.From(cmd.ModelId), ct)
            ?? throw new InvalidOperationException("Modèle introuvable.");
        model.Deactivate();
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
