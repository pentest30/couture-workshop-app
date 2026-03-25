namespace Couture.Dashboard.Contracts.Dtos;

public sealed record QuarterlyKPIsDto(
    int TotalOrders,
    decimal TotalOrdersDelta,
    int DeliveredOrders,
    int LateOrders,
    decimal OnTimeDeliveryRate,
    decimal RevenueCollected,
    decimal OutstandingBalances,
    int EmbroideredOrders,
    int BeadedOrders,
    string Quarter);
