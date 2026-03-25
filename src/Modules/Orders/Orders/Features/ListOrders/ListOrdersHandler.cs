using Couture.Clients.Persistence;
using Couture.Orders.Contracts.Dtos;
using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Orders.Features.ListOrders;

public sealed class ListOrdersHandler : IQueryHandler<ListOrdersQuery, PagedResultDto<OrderSummaryDto>>
{
    private readonly OrdersDbContext _db;
    private readonly ClientsDbContext _clientsDb;

    public ListOrdersHandler(OrdersDbContext db, ClientsDbContext clientsDb)
    {
        _db = db;
        _clientsDb = clientsDb;
    }

    public async ValueTask<PagedResultDto<OrderSummaryDto>> Handle(ListOrdersQuery query, CancellationToken ct)
    {
        var q = _db.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(o => o.Code.ToLower().Contains(search));
        }

        if (query.ClientId.HasValue)
            q = q.Where(o => o.ClientId == query.ClientId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status) && OrderStatus.TryFromName(query.Status, true, out var status))
            q = q.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(query.WorkType) && Domain.WorkType.TryFromName(query.WorkType, true, out var wt))
            q = q.Where(o => o.WorkType == wt);

        if (query.ArtisanId.HasValue)
        {
            var id = query.ArtisanId.Value;
            q = q.Where(o => o.AssignedTailorId == id || o.AssignedEmbroidererId == id || o.AssignedBeaderId == id);
        }

        if (query.ViewOwnOnly && query.CurrentUserId.HasValue)
        {
            var uid = query.CurrentUserId.Value;
            q = q.Where(o => o.AssignedTailorId == uid || o.AssignedEmbroidererId == uid || o.AssignedBeaderId == uid);
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

        // Resolve client names (load all then filter in-memory to avoid EF translation issues)
        var clientIds = items.Select(i => i.ClientId).Distinct().ToHashSet();
        var allClients = await _clientsDb.Clients.AsNoTracking().ToListAsync(ct);
        var clientNames = allClients
            .Where(c => clientIds.Contains(c.Id.Value))
            .ToDictionary(c => c.Id.Value, c => c.FullName);

        items = items.Select(i => i with { ClientName = clientNames.GetValueOrDefault(i.ClientId) }).ToList();

        // Compute outstanding balances (cross-schema query to finance.payments)
        var orderIds = items.Select(i => i.Id).ToList();
        var paymentTotals = new Dictionary<Guid, decimal>();

        if (orderIds.Count > 0)
        {
            var connection = _db.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(ct);

            await using var cmd = connection.CreateCommand();
            var paramList = string.Join(",", orderIds.Select((_, idx) => $"@p{idx}"));
            cmd.CommandText = $"SELECT \"OrderId\", SUM(\"Amount\") as \"TotalPaid\" FROM finance.payments WHERE \"OrderId\" IN ({paramList}) GROUP BY \"OrderId\"";

            for (int idx = 0; idx < orderIds.Count; idx++)
            {
                var param = cmd.CreateParameter();
                param.ParameterName = $"@p{idx}";
                param.Value = orderIds[idx];
                cmd.Parameters.Add(param);
            }

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var orderId = reader.GetGuid(0);
                var totalPaid = reader.GetDecimal(1);
                paymentTotals[orderId] = totalPaid;
            }
        }

        items = items.Select(i =>
        {
            var totalPaid = paymentTotals.GetValueOrDefault(i.Id, 0);
            return i with { OutstandingBalance = i.TotalPrice - totalPaid };
        }).ToList();

        return new PagedResultDto<OrderSummaryDto>(items, totalCount, query.Page, query.PageSize);
    }
}
