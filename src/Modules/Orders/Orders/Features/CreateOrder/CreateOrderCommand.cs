using Mediator;

namespace Couture.Orders.Features.CreateOrder;

public sealed record CreateOrderCommand(
    Guid ClientId,
    string WorkType,
    DateOnly ExpectedDeliveryDate,
    decimal TotalPrice,
    decimal? InitialDeposit,
    string? DepositPaymentMethod,
    string? Description,
    string? Fabric,
    string? TechnicalNotes,
    Guid? AssignedTailorId,
    Guid? AssignedEmbroidererId,
    Guid? AssignedBeaderId,
    string? EmbroideryStyle,
    string? ThreadColors,
    string? Density,
    string? EmbroideryZone,
    string? BeadType,
    string? Arrangement,
    string? AffectedZones) : ICommand<CreateOrderResult>;

public sealed record CreateOrderResult(Guid OrderId, string Code, string Status, decimal OutstandingBalance);
