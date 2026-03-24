using Couture.Orders.Contracts.Dtos;
using Mediator;

namespace Couture.Orders.Features.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderDetailDto?>;
