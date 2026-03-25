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

    public async Task ExecuteAsync()
    {
        // Get stall thresholds from config (N04)
        var config = await _notifDb.NotificationConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Type == NotificationType.N04_Stalled);

        if (config is not null && !config.IsEnabled) return;

        var activeOrders = await _ordersDb.Orders
            .AsNoTracking()
            .Include(o => o.Transitions)
            .Where(o => o.Status != OrderStatus.Livree && o.Status != OrderStatus.Recue)
            .ToListAsync();

        _logger.LogInformation("Evaluating {Count} active orders for stalled status", activeOrders.Count);

        foreach (var order in activeOrders)
        {
            var lastTransition = order.Transitions
                .OrderByDescending(t => t.TransitionedAt)
                .FirstOrDefault();

            if (lastTransition is null) continue;

            var daysInStatus = (int)(DateTimeOffset.UtcNow - lastTransition.TransitionedAt).TotalDays;

            var threshold = order.WorkType.Name switch
            {
                "Simple" => config?.StallThresholdSimple ?? 3,
                "Brode" => config?.StallThresholdEmbroidered ?? 7,
                "Perle" => config?.StallThresholdBeaded ?? 10,
                "Mixte" => config?.StallThresholdMixed ?? 14,
                _ => order.WorkType.StallThresholdDays,
            };

            if (daysInStatus > threshold && order.AssignedTailorId.HasValue)
            {
                await _notificationService.CreateAndSendAsync(
                    NotificationType.N04_Stalled, order.Id.Value, order.AssignedTailorId.Value,
                    $"Commande {order.Code} bloquée",
                    $"Commande {order.Code} ({order.WorkType.Label}) bloquée depuis {daysInStatus} jours en statut {order.Status.Label}. Seuil: {threshold} jours.");
            }
        }
    }
}
