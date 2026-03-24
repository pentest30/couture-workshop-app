using Ardalis.SmartEnum;
namespace Couture.Notifications.Domain;

public sealed class NotificationType : SmartEnum<NotificationType>
{
    public static readonly NotificationType N01_Overdue = new("N01_Overdue", 1, "D\u00e9lai d\u00e9pass\u00e9", NotificationPriority.Critical, true);
    public static readonly NotificationType N02_DueIn24h = new("N02_DueIn24h", 2, "Livraison dans 24h", NotificationPriority.High, true);
    public static readonly NotificationType N03_DueIn48h = new("N03_DueIn48h", 3, "Livraison dans 48h", NotificationPriority.Medium, false);
    public static readonly NotificationType N04_Stalled = new("N04_Stalled", 4, "Commande bloqu\u00e9e", NotificationPriority.High, false);
    public static readonly NotificationType N05_Retouche = new("N05_Retouche", 5, "Passage en retouche", NotificationPriority.High, true);
    public static readonly NotificationType N06_Ready = new("N06_Ready", 6, "Commande pr\u00eate", NotificationPriority.Medium, false);
    public static readonly NotificationType N07_Assigned = new("N07_Assigned", 7, "Nouvelle assignation", NotificationPriority.Medium, true);
    public static readonly NotificationType N08_UnpaidDelivery = new("N08_UnpaidDelivery", 8, "Livraison solde impay\u00e9", NotificationPriority.Critical, false);

    public string Label { get; }
    public NotificationPriority DefaultPriority { get; }
    public bool DefaultSmsEnabled { get; }

    private NotificationType(string name, int value, string label, NotificationPriority priority, bool smsEnabled) : base(name, value)
    {
        Label = label;
        DefaultPriority = priority;
        DefaultSmsEnabled = smsEnabled;
    }
}

public enum NotificationPriority { Medium = 1, High = 2, Critical = 3 }
public enum SmsDeliveryStatus { Pending = 0, Sent = 1, Delivered = 2, Failed = 3 }
