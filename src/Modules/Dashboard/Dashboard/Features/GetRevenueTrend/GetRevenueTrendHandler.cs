using Couture.Dashboard.Contracts.Dtos;
using Couture.Finance.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Dashboard.Features.GetRevenueTrend;

public sealed class GetRevenueTrendHandler : IQueryHandler<GetRevenueTrendQuery, RevenueTrendDto>
{
    private readonly FinanceDbContext _db;
    public GetRevenueTrendHandler(FinanceDbContext db) => _db = db;

    public async ValueTask<RevenueTrendDto> Handle(GetRevenueTrendQuery query, CancellationToken ct)
    {
        var quarters = new List<QuarterRevenueDto>();
        for (int i = 3; i >= 0; i--)
        {
            var q = query.Quarter - i;
            var y = query.Year;
            while (q <= 0) { q += 4; y--; }
            var startMonth = (q - 1) * 3 + 1;
            var start = new DateOnly(y, startMonth, 1);
            var end = start.AddMonths(3).AddDays(-1);
            var revenue = await _db.Payments.AsNoTracking()
                .Where(p => p.PaymentDate >= start && p.PaymentDate <= end).SumAsync(p => p.Amount, ct);
            quarters.Add(new QuarterRevenueDto($"T{q} {y}", revenue));
        }
        return new RevenueTrendDto(quarters);
    }
}
