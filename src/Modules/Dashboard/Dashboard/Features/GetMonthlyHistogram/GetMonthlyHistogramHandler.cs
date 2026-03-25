using Couture.Dashboard.Contracts.Dtos;
using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Dashboard.Features.GetMonthlyHistogram;

public sealed class GetMonthlyHistogramHandler : IQueryHandler<GetMonthlyHistogramQuery, MonthlyHistogramDto>
{
    private readonly OrdersDbContext _db;
    public GetMonthlyHistogramHandler(OrdersDbContext db) => _db = db;

    public async ValueTask<MonthlyHistogramDto> Handle(GetMonthlyHistogramQuery query, CancellationToken ct)
    {
        var startMonth = (query.Quarter - 1) * 3 + 1;
        var months = new List<MonthlyBarDto>();
        for (int m = 0; m < 3; m++)
        {
            var month = startMonth + m;
            var start = new DateOnly(query.Year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            var orders = await _db.Orders.AsNoTracking()
                .Where(o => o.ReceptionDate >= start && o.ReceptionDate <= end).ToListAsync(ct);
            months.Add(new MonthlyBarDto(start.ToString("MMM yyyy"),
                orders.Count(o => o.WorkType == WorkType.Simple),
                orders.Count(o => o.WorkType == WorkType.Brode),
                orders.Count(o => o.WorkType == WorkType.Perle),
                orders.Count(o => o.WorkType == WorkType.Mixte)));
        }
        return new MonthlyHistogramDto(months);
    }
}
