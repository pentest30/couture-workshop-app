using Couture.Catalog.Contracts.Dtos;
using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.GetModel;

public sealed record GetModelQuery(Guid Id) : IQuery<ModelDetailDto?>;

public sealed class GetModelHandler : IQueryHandler<GetModelQuery, ModelDetailDto?>
{
    private readonly CatalogDbContext _db;
    public GetModelHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<ModelDetailDto?> Handle(GetModelQuery query, CancellationToken ct)
    {
        var m = await _db.Models.AsNoTracking()
            .Include(m => m.Photos)
            .Include(m => m.ModelFabrics)
            .FirstOrDefaultAsync(m => m.Id == Contracts.ModelId.From(query.Id), ct);

        if (m is null) return null;

        var fabricIds = m.ModelFabrics.Select(mf => mf.FabricId).ToList();
        var fabrics = await _db.Fabrics.AsNoTracking()
            .Where(f => fabricIds.Contains(f.Id))
            .ToListAsync(ct);

        return new ModelDetailDto(
            m.Id.Value, m.Code, m.Name, m.Category.Name, m.Category.Label,
            m.WorkType, m.BasePrice, m.EstimatedDays, m.IsPublic,
            m.Description, m.CreatedAt,
            m.Photos.OrderBy(p => p.SortOrder).Select(p => new ModelPhotoDto(p.Id, p.FileName, p.StoragePath, p.SortOrder, p.UploadedAt)).ToList(),
            fabrics.Select(f => new FabricSummaryDto(f.Id.Value, f.Name, f.Type, f.Color, f.Supplier, f.PricePerMeter, f.StockMeters, f.Description, f.SwatchPath)).ToList());
    }
}
