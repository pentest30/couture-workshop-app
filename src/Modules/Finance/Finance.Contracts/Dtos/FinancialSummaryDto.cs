namespace Couture.Finance.Contracts.Dtos;

public sealed record FinancialSummaryDto(
    decimal TotalCollected,
    List<RevenueByMethodDto> PaymentsByMethod,
    decimal OutstandingBalances,
    int OutstandingOrderCount,
    List<UnpaidDeliveredDto> DeliveredWithUnpaid,
    List<RecentPaymentDto> RecentPayments);

public sealed record RevenueByMethodDto(
    string Method,
    string Label,
    decimal Total,
    decimal Percentage);

public sealed record RecentPaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    string PaymentMethodLabel,
    DateOnly PaymentDate,
    string? Note,
    string? ReceiptCode,
    DateTimeOffset CreatedAt);

public sealed record UnpaidDeliveredDto(
    Guid OrderId,
    string OrderCode,
    string? ClientName,
    decimal OutstandingAmount);
