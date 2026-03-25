using Couture.Notifications.Domain;
using Couture.Notifications.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Features.ListConfigs;

public sealed class ListConfigsHandler : IQueryHandler<ListConfigsQuery, List<NotificationConfigDto>>
{
    private readonly NotificationsDbContext _db;

    public ListConfigsHandler(NotificationsDbContext db) => _db = db;

    public async ValueTask<List<NotificationConfigDto>> Handle(ListConfigsQuery query, CancellationToken ct)
    {
        var configs = await _db.NotificationConfigs
            .AsNoTracking()
            .OrderBy(c => c.Type)
            .ToListAsync(ct);

        return configs.Select(c =>
        {
            var type = NotificationType.FromValue(c.Type.Value);
            return new NotificationConfigDto(
                type.Value,
                type.Name,
                type.Label,
                type.DefaultPriority.ToString(),
                c.IsEnabled,
                c.SmsEnabled,
                c.StallThresholdSimple,
                c.StallThresholdEmbroidered,
                c.StallThresholdBeaded,
                c.StallThresholdMixed,
                c.SmsWindowStart.ToString("HH:mm"),
                c.SmsWindowEnd.ToString("HH:mm"));
        }).ToList();
    }
}
