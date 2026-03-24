using Couture.Notifications.Contracts;
using Couture.SharedKernel;

namespace Couture.Notifications.Domain;

public sealed class Notification : AuditableEntity
{
    public NotificationId Id { get; private set; }
    public NotificationType Type { get; private set; } = default!;
    public NotificationPriority Priority { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public bool SendSms { get; private set; }
    public bool IsRead { get; private set; }
    public SmsDeliveryStatus? SmsStatus { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    private Notification() { }

    public static Notification Create(NotificationType type, Guid orderId, Guid recipientId, string title, string message, bool sendSms)
    {
        return new Notification
        {
            Id = NotificationId.From(Guid.NewGuid()),
            Type = type,
            Priority = type.DefaultPriority,
            OrderId = orderId,
            RecipientId = recipientId,
            Title = title,
            Message = message,
            SendSms = sendSms,
            IsRead = false,
            SmsStatus = sendSms ? SmsDeliveryStatus.Pending : null,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void UpdateSmsStatus(SmsDeliveryStatus status) => SmsStatus = status;
}
