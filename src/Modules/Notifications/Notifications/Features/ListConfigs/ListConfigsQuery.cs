using Mediator;

namespace Couture.Notifications.Features.ListConfigs;

public sealed record ListConfigsQuery() : IQuery<List<NotificationConfigDto>>;

public sealed record NotificationConfigDto(
    int TypeValue,
    string TypeName,
    string TypeLabel,
    string Priority,
    bool IsEnabled,
    bool SmsEnabled,
    int StallThresholdSimple,
    int StallThresholdEmbroidered,
    int StallThresholdBeaded,
    int StallThresholdMixed,
    string SmsWindowStart,
    string SmsWindowEnd);
