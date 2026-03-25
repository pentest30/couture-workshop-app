using Couture.Catalog.Contracts.Dtos;
using Couture.Catalog.Domain;
using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.ListModels;

public sealed record ListModelsQuery(string? Search, string? Category, bool? IsPublic, int Page = 1, int PageSize = 20)
    : IQuery<PagedCatalogResult<ModelSummaryDto>>;

public sealed class ListModelsHandler : IQueryHandler<ListModelsQuery, PagedCatalogResult<ModelSummaryDto>>
{
    private readonly CatalogDbContext _db;
    public ListModelsHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<PagedCatalogResult<ModelSummaryDto>> Handle(ListModelsQuery query, CancellationToken ct)
    {
        var q = _db.Models.AsNoTracking().Include(m => m.Photos).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(m => m.Name.ToLower().Contains(query.Search.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(query.Category) && ModelCategory.TryFromName(query.Category, true, out var cat))
            q = q.Where(m => m.Category == cat);

        if (query.IsPublic.HasValue)
            q = q.Where(m => m.IsPublic == query.IsPublic.Value);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(m => m.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedCatalogResult<ModelSummaryDto>(
            items.Select(m => new ModelSummaryDto(
                m.Id.Value, m.Code, m.Name, m.Category.Name, m.Category.Label,
                m.WorkType, m.BasePrice, m.EstimatedDays, m.IsPublic,
                m.Photos.OrderBy(p => p.SortOrder).FirstOrDefault()?.StoragePath,
                m.CreatedAt)).ToList(),
            total, query.Page, query.PageSize);
    }
}
