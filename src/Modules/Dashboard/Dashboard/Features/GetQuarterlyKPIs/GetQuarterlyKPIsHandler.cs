using Couture.Dashboard.Contracts.Dtos;
using Couture.Finance.Persistence;
using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Dashboard.Features.GetQuarterlyKPIs;

public sealed class GetQuarterlyKPIsHandler : IQueryHandler<GetQuarterlyKPIsQuery, QuarterlyKPIsDto>
{
    private readonly OrdersDbContext _ordersDb;
    private readonly FinanceDbContext _financeDb;

    public GetQuarterlyKPIsHandler(OrdersDbContext ordersDb, FinanceDbContext financeDb)
    {
        _ordersDb = ordersDb;
        _financeDb = financeDb;
    }

    public async ValueTask<QuarterlyKPIsDto> Handle(GetQuarterlyKPIsQuery query, CancellationToken ct)
    {
        var (start, end) = GetQuarterDates(query.Year, query.Quarter);
        var (prevStart, prevEnd) = GetQuarterDates(
            query.Quarter == 1 ? query.Year - 1 : query.Year,
            query.Quarter == 1 ? 4 : query.Quarter - 1);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var orders = await _ordersDb.Orders.AsNoTracking()
            .Where(o => o.ReceptionDate >= start && o.ReceptionDate <= end).ToListAsync(ct);
        var totalOrders = orders.Count;

        var prevCount = await _ordersDb.Orders.AsNoTracking()
            .CountAsync(o => o.ReceptionDate >= prevStart && o.ReceptionDate <= prevEnd, ct);
        var delta = prevCount > 0 ? Math.Round((decimal)(totalOrders - prevCount) / prevCount * 100, 1) : 0;

        var delivered = orders.Count(o => o.Status == OrderStatus.Livree);
        var lateOrders = await _ordersDb.Orders.AsNoTracking()
            .CountAsync(o => o.Status != OrderStatus.Livree && o.ExpectedDeliveryDate < today, ct);

        var deliveredAll = orders.Where(o => o.Status == OrderStatus.Livree).ToList();
        var onTime = deliveredAll.Count(o => o.ActualDeliveryDate.HasValue && o.ActualDeliveryDate.Value <= o.ExpectedDeliveryDate);
        var onTimeRate = deliveredAll.Count > 0 ? Math.Round((decimal)onTime / deliveredAll.Count * 100, 1) : 100m;

        var revenue = await _financeDb.Payments.AsNoTracking()
            .Where(p => p.PaymentDate >= start && p.PaymentDate <= end).SumAsync(p => p.Amount, ct);

        var activeOrderIds = await _ordersDb.Orders.AsNoTracking()
            .Where(o => o.Status != OrderStatus.Livree).Select(o => new { o.Id, o.TotalPrice }).ToListAsync(ct);
        decimal outstanding = 0;
        foreach (var ao in activeOrderIds)
        {
            var paid = await _financeDb.Payments.Where(p => p.OrderId == ao.Id.Value).SumAsync(p => p.Amount, ct);
            outstanding += ao.TotalPrice - paid;
        }

        var embroidered = orders.Count(o => o.WorkType == WorkType.Brode || o.WorkType == WorkType.Mixte);
        var beaded = orders.Count(o => o.WorkType == WorkType.Perle || o.WorkType == WorkType.Mixte);

        return new QuarterlyKPIsDto(totalOrders, delta, delivered, lateOrders, onTimeRate,
            revenue, outstanding, embroidered, beaded, $"T{query.Quarter} {query.Year}");
    }

    private static (DateOnly Start, DateOnly End) GetQuarterDates(int year, int quarter)
    {
        var startMonth = (quarter - 1) * 3 + 1;
        var start = new DateOnly(year, startMonth, 1);
        return (start, start.AddMonths(3).AddDays(-1));
    }
}
