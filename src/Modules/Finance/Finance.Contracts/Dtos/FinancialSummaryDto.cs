namespace Couture.Finance.Contracts.Dtos;

public sealed record FinancialSummaryDto(
    decimal TotalRevenue,
    List<RevenueByMethodDto> RevenueByMethod,
    decimal OutstandingBalances,
    int OutstandingOrderCount,
    List<UnpaidDeliveredDto> DeliveredWithUnpaid);

public sealed record RevenueByMethodDto(
    string Method,
    string MethodLabel,
    decimal Amount,
    decimal Percentage);

public sealed record UnpaidDeliveredDto(
    Guid OrderId,
    string OrderCode,
    string? ClientName,
    decimal OutstandingAmount);
