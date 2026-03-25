using Couture.Notifications.Domain;
using Couture.Notifications.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Features.ConfigureAlerts;

public sealed class ConfigureAlertsHandler : ICommandHandler<ConfigureAlertsCommand>
{
    private readonly NotificationsDbContext _db;

    public ConfigureAlertsHandler(NotificationsDbContext db) => _db = db;

    public async ValueTask<Unit> Handle(ConfigureAlertsCommand command, CancellationToken ct)
    {
        var type = NotificationType.FromValue(command.TypeValue);
        var config = await _db.NotificationConfigs
            .FirstOrDefaultAsync(c => c.Type == type, ct)
            ?? throw new InvalidOperationException($"Config for notification type {type.Name} not found.");

        TimeOnly? smsStart = command.SmsWindowStart is not null ? TimeOnly.Parse(command.SmsWindowStart) : null;
        TimeOnly? smsEnd = command.SmsWindowEnd is not null ? TimeOnly.Parse(command.SmsWindowEnd) : null;

        config.Update(
            command.IsEnabled, command.SmsEnabled,
            command.StallThresholdSimple, command.StallThresholdEmbroidered,
            command.StallThresholdBeaded, command.StallThresholdMixed,
            smsStart, smsEnd);

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
