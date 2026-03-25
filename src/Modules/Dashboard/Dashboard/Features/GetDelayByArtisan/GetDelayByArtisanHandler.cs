using Couture.Dashboard.Contracts.Dtos;
using Couture.Identity.Persistence;
using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Dashboard.Features.GetDelayByArtisan;

public sealed class GetDelayByArtisanHandler : IQueryHandler<GetDelayByArtisanQuery, DelayByArtisanDto>
{
    private readonly OrdersDbContext _ordersDb;
    private readonly IdentityDbContext _identityDb;

    public GetDelayByArtisanHandler(OrdersDbContext ordersDb, IdentityDbContext identityDb)
    {
        _ordersDb = ordersDb;
        _identityDb = identityDb;
    }

    public async ValueTask<DelayByArtisanDto> Handle(GetDelayByArtisanQuery query, CancellationToken ct)
    {
        var startMonth = (query.Quarter - 1) * 3 + 1;
        var start = new DateOnly(query.Year, startMonth, 1);
        var end = start.AddMonths(3).AddDays(-1);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var lateDelivered = await _ordersDb.Orders.AsNoTracking()
            .Where(o => o.ReceptionDate >= start && o.ReceptionDate <= end)
            .Where(o => o.Status == OrderStatus.Livree && o.ActualDeliveryDate.HasValue && o.ActualDeliveryDate.Value > o.ExpectedDeliveryDate)
            .Where(o => o.AssignedTailorId.HasValue)
            .Select(o => new { TailorId = o.AssignedTailorId!.Value, DelayDays = o.ActualDeliveryDate!.Value.DayNumber - o.ExpectedDeliveryDate.DayNumber })
            .ToListAsync(ct);

        var activeLate = await _ordersDb.Orders.AsNoTracking()
            .Where(o => o.ReceptionDate >= start && o.ReceptionDate <= end)
            .Where(o => o.Status != OrderStatus.Livree && o.ExpectedDeliveryDate < today && o.AssignedTailorId.HasValue)
            .Select(o => new { TailorId = o.AssignedTailorId!.Value, DelayDays = today.DayNumber - o.ExpectedDeliveryDate.DayNumber })
            .ToListAsync(ct);

        var grouped = lateDelivered.Concat(activeLate)
            .GroupBy(o => o.TailorId)
            .Select(g => new { g.Key, Avg = Math.Round((decimal)g.Average(x => x.DelayDays), 1) })
            .OrderByDescending(x => x.Avg).Take(5).ToList();

        var tailorIds = grouped.Select(g => g.Key).ToList();
        var tailors = await _identityDb.Users.AsNoTracking()
            .Where(u => tailorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FirstName + " " + u.LastName, ct);

        return new DelayByArtisanDto(grouped.Select(g =>
            new ArtisanDelayDto(tailors.GetValueOrDefault(g.Key, "Inconnu"), g.Avg)).ToList());
    }
}
