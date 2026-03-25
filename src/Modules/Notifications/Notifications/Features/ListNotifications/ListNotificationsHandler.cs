using Couture.Notifications.Domain;
using Couture.Notifications.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Features.ListNotifications;

public sealed class ListNotificationsHandler : IQueryHandler<ListNotificationsQuery, ListNotificationsResult>
{
    private readonly NotificationsDbContext _db;

    public ListNotificationsHandler(NotificationsDbContext db) => _db = db;

    public async ValueTask<ListNotificationsResult> Handle(ListNotificationsQuery query, CancellationToken ct)
    {
        var q = _db.Notifications.AsNoTracking()
            .Where(n => n.RecipientId == query.RecipientId);

        if (query.Filter == "unread")
            q = q.Where(n => !n.IsRead);
        else if (query.Filter == "critical")
            q = q.Where(n => n.Priority == NotificationPriority.Critical);

        var totalCount = await q.CountAsync(ct);
        var unreadCount = await _db.Notifications.AsNoTracking()
            .CountAsync(n => n.RecipientId == query.RecipientId && !n.IsRead, ct);

        var items = await q
            .OrderByDescending(n => n.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new NotificationDto(
                n.Id.Value, n.Type.Name, n.Type.Label,
                n.Priority.ToString(), n.Title, n.Message,
                n.OrderId, n.IsRead, n.CreatedAt, n.ReadAt))
            .ToListAsync(ct);

        return new ListNotificationsResult(items, totalCount, unreadCount);
    }
}
