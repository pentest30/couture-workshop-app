using Couture.Orders.Contracts.Dtos;
using Mediator;

namespace Couture.Orders.Features.ListOrders;

public sealed record ListOrdersQuery(
    string? Search,
    string? Status,
    string? WorkType,
    Guid? ArtisanId,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    bool? LateOnly,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "createdAt",
    string SortDir = "desc",
    Guid? CurrentUserId = null,
    bool ViewOwnOnly = false) : IQuery<PagedResultDto<OrderSummaryDto>>;
