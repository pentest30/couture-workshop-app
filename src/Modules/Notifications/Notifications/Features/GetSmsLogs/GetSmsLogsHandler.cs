using Couture.Notifications.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Features.GetSmsLogs;

public sealed class GetSmsLogsHandler : IQueryHandler<GetSmsLogsQuery, SmsLogsResult>
{
    private readonly NotificationsDbContext _db;

    public GetSmsLogsHandler(NotificationsDbContext db) => _db = db;

    public async ValueTask<SmsLogsResult> Handle(GetSmsLogsQuery query, CancellationToken ct)
    {
        var q = _db.Notifications.AsNoTracking()
            .Where(n => n.SendSms);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(n => n.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new SmsLogDto(
                n.Id.Value, n.Type.Name, n.RecipientId,
                n.Title, n.SmsStatus != null ? n.SmsStatus.Value.ToString() : "N/A",
                n.CreatedAt))
            .ToListAsync(ct);

        return new SmsLogsResult(items, total);
    }
}
