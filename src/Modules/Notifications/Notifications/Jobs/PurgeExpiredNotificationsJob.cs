using Couture.Notifications.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Couture.Notifications.Jobs;

public sealed class PurgeExpiredNotificationsJob
{
    private readonly NotificationsDbContext _db;
    private readonly ILogger<PurgeExpiredNotificationsJob> _logger;

    public PurgeExpiredNotificationsJob(NotificationsDbContext db, ILogger<PurgeExpiredNotificationsJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var expired = await _db.Notifications
            .Where(n => n.ExpiresAt < DateTimeOffset.UtcNow)
            .ToListAsync();

        if (expired.Count > 0)
        {
            _db.Notifications.RemoveRange(expired);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Purged {Count} expired notifications", expired.Count);
        }
    }
}
