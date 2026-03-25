using Couture.Notifications.Contracts;
using Couture.Notifications.Persistence;
using Couture.Notifications.Sms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Couture.Notifications.Domain;

public sealed class NotificationService
{
    private readonly NotificationsDbContext _db;
    private readonly ISmsGateway _smsGateway;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(NotificationsDbContext db, ISmsGateway smsGateway, ILogger<NotificationService> logger)
    {
        _db = db;
        _smsGateway = smsGateway;
        _logger = logger;
    }

    public async Task CreateAndSendAsync(
        NotificationType type, Guid orderId, Guid recipientId,
        string title, string message, string? recipientPhone = null,
        CancellationToken ct = default)
    {
        // Check if this notification type is enabled
        var config = await _db.NotificationConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Type == type, ct);

        if (config is not null && !config.IsEnabled)
        {
            _logger.LogInformation("Notification {Type} is disabled, skipping", type.Name);
            return;
        }

        var sendSms = config?.SmsEnabled ?? type.DefaultSmsEnabled;

        var notification = Notification.Create(type, orderId, recipientId, title, message, sendSms);
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);

        // Send SMS if enabled and phone provided
        if (sendSms && !string.IsNullOrWhiteSpace(recipientPhone))
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var windowStart = config?.SmsWindowStart ?? new TimeOnly(8, 0);
            var windowEnd = config?.SmsWindowEnd ?? new TimeOnly(20, 0);

            if (now >= windowStart && now <= windowEnd)
            {
                var result = await _smsGateway.SendAsync(recipientPhone, message, ct);
                notification.UpdateSmsStatus(result.Success ? SmsDeliveryStatus.Sent : SmsDeliveryStatus.Failed);
                await _db.SaveChangesAsync(ct);
                _logger.LogInformation("SMS {Status} for notification {Id} to {Phone}",
                    result.Success ? "sent" : "failed", notification.Id.Value, recipientPhone);
            }
            else
            {
                _logger.LogInformation("SMS queued (outside window {Start}-{End}) for notification {Id}",
                    windowStart, windowEnd, notification.Id.Value);
            }
        }
    }
}
