using Couture.Catalog.Contracts.Dtos;
using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.GetFabric;

public sealed record GetFabricQuery(Guid Id) : IQuery<FabricSummaryDto?>;

public sealed class GetFabricHandler : IQueryHandler<GetFabricQuery, FabricSummaryDto?>
{
    private readonly CatalogDbContext _db;
    public GetFabricHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<FabricSummaryDto?> Handle(GetFabricQuery query, CancellationToken ct)
    {
        var f = await _db.Fabrics.AsNoTracking().FirstOrDefaultAsync(f => f.Id == Contracts.FabricId.From(query.Id), ct);
        if (f is null) return null;
        return new FabricSummaryDto(f.Id.Value, f.Name, f.Type, f.Color, f.Supplier, f.PricePerMeter, f.StockMeters, f.Description, f.SwatchPath);
    }
}
