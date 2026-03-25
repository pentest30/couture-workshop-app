using Couture.Finance.Contracts.Dtos;
using Couture.Finance.Domain;
using Couture.Finance.Persistence;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Finance.Features.GetFinancialSummary;

public sealed class GetFinancialSummaryHandler : IQueryHandler<GetFinancialSummaryQuery, FinancialSummaryDto>
{
    private readonly FinanceDbContext _financeDb;
    private readonly OrdersDbContext _ordersDb;

    public GetFinancialSummaryHandler(FinanceDbContext financeDb, OrdersDbContext ordersDb)
    {
        _financeDb = financeDb;
        _ordersDb = ordersDb;
    }

    public async ValueTask<FinancialSummaryDto> Handle(GetFinancialSummaryQuery query, CancellationToken ct)
    {
        var (startDate, endDate) = GetQuarterDates(query.Year, query.Quarter);

        // Revenue in quarter
        var payments = await _financeDb.Payments
            .AsNoTracking()
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .ToListAsync(ct);

        var totalRevenue = payments.Sum(p => p.Amount);

        // Revenue by method
        var byMethod = payments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new RevenueByMethodDto(
                g.Key.Name,
                g.Key.Label,
                g.Sum(p => p.Amount),
                totalRevenue > 0 ? Math.Round(g.Sum(p => p.Amount) / totalRevenue * 100, 1) : 0))
            .OrderByDescending(r => r.Amount)
            .ToList();

        // Outstanding balances (orders not delivered)
        var activeOrders = await _ordersDb.Orders
            .AsNoTracking()
            .Where(o => !o.HasUnpaidBalance && o.Status.Value != 8) // Not Livree
            .Select(o => new { o.Id, o.Code, o.TotalPrice, o.ClientId })
            .ToListAsync(ct);

        decimal outstandingTotal = 0;
        int outstandingCount = 0;

        foreach (var order in activeOrders)
        {
            var paid = await _financeDb.Payments
                .Where(p => p.OrderId == order.Id.Value)
                .SumAsync(p => p.Amount, ct);
            var remaining = order.TotalPrice - paid;
            if (remaining > 0)
            {
                outstandingTotal += remaining;
                outstandingCount++;
            }
        }

        // Delivered with unpaid
        var deliveredUnpaid = await _ordersDb.Orders
            .AsNoTracking()
            .Where(o => o.HasUnpaidBalance)
            .Select(o => new { o.Id, o.Code, o.ClientId, o.TotalPrice })
            .ToListAsync(ct);

        var unpaidList = new List<UnpaidDeliveredDto>();
        foreach (var order in deliveredUnpaid)
        {
            var paid = await _financeDb.Payments
                .Where(p => p.OrderId == order.Id.Value)
                .SumAsync(p => p.Amount, ct);
            unpaidList.Add(new UnpaidDeliveredDto(
                order.Id.Value, order.Code, null, order.TotalPrice - paid));
        }

        return new FinancialSummaryDto(
            totalRevenue, byMethod, outstandingTotal, outstandingCount, unpaidList);
    }

    private static (DateOnly Start, DateOnly End) GetQuarterDates(int year, int quarter)
    {
        var startMonth = (quarter - 1) * 3 + 1;
        var start = new DateOnly(year, startMonth, 1);
        var end = start.AddMonths(3).AddDays(-1);
        return (start, end);
    }
}
