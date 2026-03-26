using Couture.Notifications.Domain;
using Couture.Notifications.Persistence;
using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Couture.Notifications.Jobs;

public sealed class EvaluateStalledOrdersJob
{
    private readonly OrdersDbContext _ordersDb;
    private readonly NotificationsDbContext _notifDb;
    private readonly NotificationService _notificationService;
    private readonly ILogger<EvaluateStalledOrdersJob> _logger;

    public EvaluateStalledOrdersJob(OrdersDbContext ordersDb, NotificationsDbContext notifDb,
        NotificationService notificationService, ILogger<EvaluateStalledOrdersJob> logger)
    {
        _ordersDb = ordersDb;
        _notifDb = notifDb;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// Default stall threshold: 7 days for any status/worktype combination.
    private const int DefaultThresholdDays = 7;

    public async Task ExecuteAsync()
    {
        var config = await _notifDb.NotificationConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Type == NotificationType.N04_Stalled);

        if (config is not null && !config.IsEnabled) return;

        var activeOrders = await _ordersDb.Orders
            .AsNoTracking()
            .Include(o => o.Transitions)
            .Where(o => o.Status != OrderStatus.Livree && o.Status != OrderStatus.Recue)
            .ToListAsync();

        _logger.LogInformation("Evaluating {Count} active orders for per-status stall thresholds", activeOrders.Count);

        foreach (var order in activeOrders)
        {
            var lastTransition = order.Transitions
                .OrderByDescending(t => t.TransitionedAt)
                .FirstOrDefault();

            if (lastTransition is null) continue;

            var daysInStatus = (int)(DateTimeOffset.UtcNow - lastTransition.TransitionedAt).TotalDays;
            var threshold = DefaultThresholdDays;

            if (daysInStatus <= threshold) continue;

            var title = $"Commande {order.Code} — statut {order.Status.Label} trop long";
            var message = $"Commande {order.Code} ({order.WorkType.Label}) en statut {order.Status.Label} "
                + $"depuis {daysInStatus} jours (seuil: {threshold}j).";

            // Notify managers
            await _notificationService.CreateAndSendToManagersAsync(
                NotificationType.N04_Stalled, order.Id.Value, title, message);

            // Notify the assigned tailor directly
            if (order.AssignedTailorId.HasValue)
            {
                await _notificationService.CreateAndSendAsync(
                    NotificationType.N04_Stalled, order.Id.Value, order.AssignedTailorId.Value,
                    title, message);
            }
        }
    }
}
