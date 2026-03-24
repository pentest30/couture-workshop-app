using Couture.SharedKernel;

namespace Couture.Orders.Contracts.Events;

public sealed record OrderDeliveredEvent(
    OrderId OrderId,
    string OrderCode,
    bool HasUnpaidBalance,
    string? UnpaidReason) : IDomainEvent;
