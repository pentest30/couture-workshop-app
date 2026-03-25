using Couture.Notifications.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Features.GetUnreadCount;

public sealed class GetUnreadCountHandler : IQueryHandler<GetUnreadCountQuery, int>
{
    private readonly NotificationsDbContext _db;

    public GetUnreadCountHandler(NotificationsDbContext db) => _db = db;

    public async ValueTask<int> Handle(GetUnreadCountQuery query, CancellationToken ct)
    {
        return await _db.Notifications.AsNoTracking()
            .CountAsync(n => n.RecipientId == query.RecipientId && !n.IsRead, ct);
    }
}
