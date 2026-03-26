namespace Couture.Notifications.Domain;

public sealed class NotificationConfig
{
    public int Id { get; private set; }
    public NotificationType Type { get; private set; } = default!;
    public bool IsEnabled { get; private set; } = true;
    public bool SmsEnabled { get; private set; }
    public int StallThresholdSimple { get; private set; } = 7;
    public int StallThresholdEmbroidered { get; private set; } = 7;
    public int StallThresholdBeaded { get; private set; } = 7;
    public int StallThresholdMixed { get; private set; } = 7;
    public TimeOnly SmsWindowStart { get; private set; } = new(8, 0);
    public TimeOnly SmsWindowEnd { get; private set; } = new(20, 0);

    private NotificationConfig() { }

    public static NotificationConfig Create(NotificationType type)
    {
        return new NotificationConfig
        {
            Type = type,
            IsEnabled = true,
            SmsEnabled = type.DefaultSmsEnabled,
        };
    }

    public void Update(bool? isEnabled = null, bool? smsEnabled = null,
        int? stallSimple = null, int? stallEmbroidered = null, int? stallBeaded = null, int? stallMixed = null,
        TimeOnly? smsStart = null, TimeOnly? smsEnd = null)
    {
        if (isEnabled.HasValue) IsEnabled = isEnabled.Value;
        if (smsEnabled.HasValue) SmsEnabled = smsEnabled.Value;
        if (stallSimple.HasValue) StallThresholdSimple = stallSimple.Value;
        if (stallEmbroidered.HasValue) StallThresholdEmbroidered = stallEmbroidered.Value;
        if (stallBeaded.HasValue) StallThresholdBeaded = stallBeaded.Value;
        if (stallMixed.HasValue) StallThresholdMixed = stallMixed.Value;
        if (smsStart.HasValue) SmsWindowStart = smsStart.Value;
        if (smsEnd.HasValue) SmsWindowEnd = smsEnd.Value;
    }
}
