namespace Couture.Catalog.Contracts.Dtos;

public sealed record FabricSummaryDto(
    Guid Id, string Name, string Type, string Color,
    string? Supplier, decimal PricePerMeter, decimal StockMeters,
    string? Description, string? SwatchPath);

public sealed record PagedCatalogResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
