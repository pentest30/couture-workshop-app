namespace Couture.Notifications.Contracts;
public readonly record struct NotificationId(Guid Value)
{
    public static NotificationId From(Guid value) => new(value);
    public static NotificationId NewNotificationId() => new(Guid.NewGuid());
}
