namespace Couture.Catalog.Contracts.Dtos;

public sealed record ModelDetailDto(
    Guid Id, string Code, string Name, string Category, string CategoryLabel,
    string WorkType, decimal BasePrice, int EstimatedDays, bool IsPublic,
    string? Description, DateTimeOffset CreatedAt,
    List<ModelPhotoDto> Photos, List<FabricSummaryDto> Fabrics);

public sealed record ModelPhotoDto(Guid Id, string FileName, string StoragePath, int SortOrder, DateTimeOffset UploadedAt);
