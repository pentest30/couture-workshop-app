using Couture.Catalog.Contracts;
using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.LinkFabric;

public sealed record LinkFabricCommand(Guid ModelId, Guid FabricId) : ICommand;
public sealed record UnlinkFabricCommand(Guid ModelId, Guid FabricId) : ICommand;

public sealed class LinkFabricHandler : ICommandHandler<LinkFabricCommand>
{
    private readonly CatalogDbContext _db;
    public LinkFabricHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<Unit> Handle(LinkFabricCommand cmd, CancellationToken ct)
    {
        var model = await _db.Models.Include(m => m.ModelFabrics).FirstOrDefaultAsync(m => m.Id == Contracts.ModelId.From(cmd.ModelId), ct)
            ?? throw new InvalidOperationException("Model not found.");
        model.LinkFabric(FabricId.From(cmd.FabricId));
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public sealed class UnlinkFabricHandler : ICommandHandler<UnlinkFabricCommand>
{
    private readonly CatalogDbContext _db;
    public UnlinkFabricHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<Unit> Handle(UnlinkFabricCommand cmd, CancellationToken ct)
    {
        var model = await _db.Models.Include(m => m.ModelFabrics).FirstOrDefaultAsync(m => m.Id == Contracts.ModelId.From(cmd.ModelId), ct)
            ?? throw new InvalidOperationException("Model not found.");
        model.UnlinkFabric(FabricId.From(cmd.FabricId));
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
