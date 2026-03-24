using Couture.Orders.Contracts.Dtos;
using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Orders.Features.ListOrders;

public sealed class ListOrdersHandler : IQueryHandler<ListOrdersQuery, PagedResultDto<OrderSummaryDto>>
{
    private readonly OrdersDbContext _db;

    public ListOrdersHandler(OrdersDbContext db) => _db = db;

    public async ValueTask<PagedResultDto<OrderSummaryDto>> Handle(ListOrdersQuery query, CancellationToken ct)
    {
        var q = _db.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(o => o.Code.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && OrderStatus.TryFromName(query.Status, true, out var status))
            q = q.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(query.WorkType) && Domain.WorkType.TryFromName(query.WorkType, true, out var wt))
            q = q.Where(o => o.WorkType == wt);

        if (query.ArtisanId.HasValue)
        {
            var id = query.ArtisanId.Value;
            q = q.Where(o => o.AssignedTailorId == id || o.AssignedEmbroidererId == id || o.AssignedBeaderId == id);
        }

        if (query.DateFrom.HasValue)
            q = q.Where(o => o.ReceptionDate >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            q = q.Where(o => o.ReceptionDate <= query.DateTo.Value);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (query.LateOnly == true)
            q = q.Where(o => !o.Status.IsTerminal && o.ExpectedDeliveryDate < today);

        var totalCount = await q.CountAsync(ct);

        q = query.SortBy?.ToLower() switch
        {
            "expecteddeliverydate" => query.SortDir == "asc" ? q.OrderBy(o => o.ExpectedDeliveryDate) : q.OrderByDescending(o => o.ExpectedDeliveryDate),
            "status" => query.SortDir == "asc" ? q.OrderBy(o => o.Status) : q.OrderByDescending(o => o.Status),
            _ => query.SortDir == "asc" ? q.OrderBy(o => o.CreatedAt) : q.OrderByDescending(o => o.CreatedAt),
        };

        var orders = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var items = orders.Select(o =>
        {
            var isLate = !o.Status.IsTerminal && o.ExpectedDeliveryDate < today;
            var delay = isLate ? (today.DayNumber - o.ExpectedDeliveryDate.DayNumber) : 0;
            return new OrderSummaryDto(
                o.Id.Value, o.Code, o.ClientId, null,
                o.Status.Name, o.Status.Label, o.Status.Color,
                o.WorkType.Name, o.WorkType.Label,
                o.ExpectedDeliveryDate, delay, isLate,
                o.TotalPrice, 0,
                null, o.CreatedAt);
        }).ToList();

        return new PagedResultDto<OrderSummaryDto>(items, totalCount, query.Page, query.PageSize);
    }
}
