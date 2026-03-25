using Mediator;

namespace Couture.Notifications.Features.ConfigureAlerts;

public sealed record ConfigureAlertsCommand(
    int TypeValue,
    bool? IsEnabled,
    bool? SmsEnabled,
    int? StallThresholdSimple,
    int? StallThresholdEmbroidered,
    int? StallThresholdBeaded,
    int? StallThresholdMixed,
    string? SmsWindowStart,
    string? SmsWindowEnd) : ICommand;
