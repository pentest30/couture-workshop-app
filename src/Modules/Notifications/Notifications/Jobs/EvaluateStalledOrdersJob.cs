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

    /// <summary>
    /// Expected duration (days) per status per work type.
    /// Key: (StatusName, WorkTypeName) → days.
    /// If a specific combo is missing, falls back to the status-only default.
    /// </summary>
    private static readonly Dictionary<(string Status, string WorkType), int> StatusThresholds = new()
    {
        // EnAttente — should not stay long regardless of type
        { ("EnAttente", "Simple"), 2 },
        { ("EnAttente", "Brode"),  2 },
        { ("EnAttente", "Perle"),  2 },
        { ("EnAttente", "Mixte"),  3 },

        // EnCours (couture)
        { ("EnCours", "Simple"), 3 },
        { ("EnCours", "Brode"),  5 },
        { ("EnCours", "Perle"),  5 },
        { ("EnCours", "Mixte"),  7 },

        // Broderie
        { ("Broderie", "Brode"), 7 },
        { ("Broderie", "Mixte"), 7 },

        // Perlage
        { ("Perlage", "Perle"), 7 },
        { ("Perlage", "Mixte"), 10 },

        // Retouche — should be quick
        { ("Retouche", "Simple"), 2 },
        { ("Retouche", "Brode"),  3 },
        { ("Retouche", "Perle"),  3 },
        { ("Retouche", "Mixte"),  4 },

        // Prete — waiting for client pickup
        { ("Prete", "Simple"), 5 },
        { ("Prete", "Brode"),  5 },
        { ("Prete", "Perle"),  5 },
        { ("Prete", "Mixte"),  5 },
    };

    private static int GetThreshold(string status, string workType)
    {
        if (StatusThresholds.TryGetValue((status, workType), out var days))
            return days;
        // Fallback: 5 days
        return 5;
    }

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
            var threshold = GetThreshold(order.Status.Name, order.WorkType.Name);

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
