using Couture.Notifications.Domain;
using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Couture.Notifications.Jobs;

public sealed class EvaluateOverdueOrdersJob
{
    private readonly OrdersDbContext _ordersDb;
    private readonly NotificationService _notificationService;
    private readonly ILogger<EvaluateOverdueOrdersJob> _logger;

    public EvaluateOverdueOrdersJob(OrdersDbContext ordersDb, NotificationService notificationService, ILogger<EvaluateOverdueOrdersJob> logger)
    {
        _ordersDb = ordersDb;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tomorrow = today.AddDays(1);
        var dayAfterTomorrow = today.AddDays(2);

        var activeOrders = await _ordersDb.Orders
            .AsNoTracking()
            .Where(o => o.Status != OrderStatus.Livree)
            .ToListAsync();

        _logger.LogInformation("Evaluating {Count} active orders for overdue/deadline notifications", activeOrders.Count);

        foreach (var order in activeOrders)
        {
            // N01: Overdue (delivery date passed)
            if (order.ExpectedDeliveryDate < today)
            {
                var daysLate = (today.DayNumber - order.ExpectedDeliveryDate.DayNumber);
                await _notificationService.CreateAndSendToManagersAsync(
                    NotificationType.N01_Overdue, order.Id.Value,
                    $"Commande {order.Code} en retard",
                    $"Commande {order.Code} — délai dépassé de {daysLate} jour(s). Statut: {order.Status.Label}.");
            }
            // N02: Due in 24h
            else if (order.ExpectedDeliveryDate == tomorrow)
            {
                await _notificationService.CreateAndSendToManagersAsync(
                    NotificationType.N02_DueIn24h, order.Id.Value,
                    $"Commande {order.Code} — livraison demain",
                    $"Commande {order.Code} doit être livrée demain {order.ExpectedDeliveryDate:dd/MM/yyyy}.");
            }
            // N03: Due in 48h
            else if (order.ExpectedDeliveryDate == dayAfterTomorrow)
            {
                await _notificationService.CreateAndSendToManagersAsync(
                    NotificationType.N03_DueIn48h, order.Id.Value,
                    $"Commande {order.Code} — livraison dans 2 jours",
                    $"Commande {order.Code} doit être livrée le {order.ExpectedDeliveryDate:dd/MM/yyyy}.");
            }
        }
    }
}
