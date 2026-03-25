using Couture.Dashboard.Contracts.Dtos;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Dashboard.Features.GetStatusDistribution;

public sealed class GetStatusDistributionHandler : IQueryHandler<GetStatusDistributionQuery, StatusDistributionDto>
{
    private readonly OrdersDbContext _db;
    public GetStatusDistributionHandler(OrdersDbContext db) => _db = db;

    public async ValueTask<StatusDistributionDto> Handle(GetStatusDistributionQuery query, CancellationToken ct)
    {
        var startMonth = (query.Quarter - 1) * 3 + 1;
        var start = new DateOnly(query.Year, startMonth, 1);
        var end = start.AddMonths(3).AddDays(-1);
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.ReceptionDate >= start && o.ReceptionDate <= end).ToListAsync(ct);
        var total = orders.Count;
        var slices = orders.GroupBy(o => o.Status)
            .Select(g => new StatusSliceDto(g.Key.Name, g.Key.Label, g.Key.Color, g.Count(),
                total > 0 ? Math.Round((decimal)g.Count() / total * 100, 1) : 0))
            .OrderByDescending(s => s.Count).ToList();
        return new StatusDistributionDto(slices);
    }
}
