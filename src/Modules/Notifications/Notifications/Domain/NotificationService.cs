using Couture.Notifications.Contracts;
using Couture.Notifications.Hub;
using Couture.Notifications.Persistence;
using Couture.Notifications.Sms;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Couture.Notifications.Domain;

public sealed class NotificationService
{
    private readonly NotificationsDbContext _db;
    private readonly ISmsGateway _smsGateway;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IManagerResolver _managerResolver;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        NotificationsDbContext db,
        ISmsGateway smsGateway,
        IHubContext<NotificationHub> hubContext,
        IManagerResolver managerResolver,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _smsGateway = smsGateway;
        _hubContext = hubContext;
        _managerResolver = managerResolver;
        _logger = logger;
    }

    public async Task CreateAndSendAsync(
        NotificationType type, Guid orderId, Guid recipientId,
        string title, string message, string? recipientPhone = null,
        CancellationToken ct = default)
    {
        // Deduplicate: skip if same type+order+recipient already exists
        var existingTypes = await _db.Notifications
            .Where(n => n.OrderId == orderId && n.RecipientId == recipientId)
            .Select(n => n.Type)
            .ToListAsync(ct);
        if (existingTypes.Any(t => t == type))
        {
            _logger.LogInformation("Notification {Type} already exists for order {OrderId}, skipping duplicate", type.Name, orderId);
            return;
        }

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

        // Broadcast via SignalR to the recipient
        await BroadcastNotificationAsync(notification, ct);

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

    /// <summary>
    /// Creates and sends the notification to all users with the Manager role.
    /// </summary>
    public async Task CreateAndSendToManagersAsync(
        NotificationType type, Guid orderId,
        string title, string message, string? recipientPhone = null,
        CancellationToken ct = default)
    {
        var managerIds = await _managerResolver.GetManagerIdsAsync(ct);
        foreach (var managerId in managerIds)
        {
            await CreateAndSendAsync(type, orderId, managerId, title, message, recipientPhone, ct);
        }
    }

    private async Task BroadcastNotificationAsync(Notification notification, CancellationToken ct)
    {
        try
        {
            var payload = new
            {
                id = notification.Id.Value,
                type = notification.Type.Name,
                typeLabel = notification.Type.Label,
                priority = notification.Priority.ToString(),
                title = notification.Title,
                message = notification.Message,
                orderId = notification.OrderId,
                isRead = false,
                createdAt = notification.CreatedAt,
            };

            await _hubContext.Clients
                .Group($"user-{notification.RecipientId}")
                .SendAsync("NotificationReceived", payload, ct);

            _logger.LogInformation("SignalR broadcast sent for notification {Id} to user {UserId}",
                notification.Id.Value, notification.RecipientId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast notification {Id} via SignalR", notification.Id.Value);
        }
    }
}

