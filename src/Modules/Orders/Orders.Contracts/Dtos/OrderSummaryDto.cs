namespace Couture.Orders.Contracts.Dtos;

public sealed record OrderSummaryDto(
    Guid Id,
    string Code,
    Guid ClientId,
    string? ClientName,
    string Status,
    string StatusLabel,
    string StatusColor,
    string WorkType,
    string WorkTypeLabel,
    DateOnly ExpectedDeliveryDate,
    int DelayDays,
    bool IsLate,
    decimal TotalPrice,
    decimal OutstandingBalance,
    string? AssignedTailorName,
    DateTimeOffset CreatedAt);
