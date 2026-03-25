using Couture.Catalog.Contracts.Dtos;
using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.ListFabrics;

public sealed record ListFabricsQuery(string? Search, int Page = 1, int PageSize = 20)
    : IQuery<PagedCatalogResult<FabricSummaryDto>>;

public sealed class ListFabricsHandler : IQueryHandler<ListFabricsQuery, PagedCatalogResult<FabricSummaryDto>>
{
    private readonly CatalogDbContext _db;
    public ListFabricsHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<PagedCatalogResult<FabricSummaryDto>> Handle(ListFabricsQuery query, CancellationToken ct)
    {
        var q = _db.Fabrics.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(f => f.Name.ToLower().Contains(s) || f.Type.ToLower().Contains(s) || f.Color.ToLower().Contains(s));
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(f => f.Name)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedCatalogResult<FabricSummaryDto>(
            items.Select(f => new FabricSummaryDto(f.Id.Value, f.Name, f.Type, f.Color, f.Supplier, f.PricePerMeter, f.StockMeters, f.Description, f.SwatchPath)).ToList(),
            total, query.Page, query.PageSize);
    }
}
