namespace Couture.Catalog.Contracts.Dtos;

public sealed record ModelSummaryDto(
    Guid Id, string Code, string Name, string Category, string CategoryLabel,
    string WorkType, decimal BasePrice, int EstimatedDays, bool IsPublic,
    string? PrimaryPhotoPath, DateTimeOffset CreatedAt);
