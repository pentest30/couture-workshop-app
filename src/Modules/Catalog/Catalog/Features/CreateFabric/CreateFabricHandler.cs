using Couture.Catalog.Domain;
using Couture.Catalog.Persistence;
using Mediator;

namespace Couture.Catalog.Features.CreateFabric;

public sealed record CreateFabricCommand(string Name, string Type, string Color, decimal PricePerMeter,
    decimal StockMeters, string? Supplier, string? Description) : ICommand<Guid>;

public sealed class CreateFabricHandler : ICommandHandler<CreateFabricCommand, Guid>
{
    private readonly CatalogDbContext _db;
    public CreateFabricHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<Guid> Handle(CreateFabricCommand cmd, CancellationToken ct)
    {
        var fabric = Fabric.Create(cmd.Name, cmd.Type, cmd.Color, cmd.PricePerMeter, cmd.StockMeters, cmd.Supplier, cmd.Description);
        _db.Fabrics.Add(fabric);
        await _db.SaveChangesAsync(ct);
        return fabric.Id.Value;
    }
}
