using Couture.Notifications.Domain;
using Couture.Orders.Contracts.Events;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.EventHandlers;

public sealed class StatusChangedHandler : INotificationHandler<StatusChangedEvent>
{
    private readonly NotificationService _notificationService;
    private readonly OrdersDbContext _ordersDb;

    public StatusChangedHandler(NotificationService notificationService, OrdersDbContext ordersDb)
    {
        _notificationService = notificationService;
        _ordersDb = ordersDb;
    }

    public async ValueTask Handle(StatusChangedEvent evt, CancellationToken ct)
    {
        var order = await _ordersDb.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id.Value == evt.OrderId.Value, ct);
        if (order is null) return;

        // N05: Status -> Retouche
        if (evt.ToStatus == "Retouche")
        {
            if (order.AssignedTailorId.HasValue)
            {
                await _notificationService.CreateAndSendAsync(
                    NotificationType.N05_Retouche, evt.OrderId.Value, order.AssignedTailorId.Value,
                    $"Commande {evt.OrderCode} en retouche",
                    $"Commande {evt.OrderCode} passée en retouche. Motif: {evt.Reason ?? "non spécifié"}.",
                    ct: ct);
            }
        }

        // N06: Status -> Prete
        if (evt.ToStatus == "Prete")
        {
            if (order.AssignedTailorId.HasValue)
            {
                await _notificationService.CreateAndSendAsync(
                    NotificationType.N06_Ready, evt.OrderId.Value, order.AssignedTailorId.Value,
                    $"Commande {evt.OrderCode} prête",
                    $"Commande {evt.OrderCode} est prête pour la livraison.",
                    ct: ct);
            }
        }

        // N07: New artisan assigned (Broderie or Perlage phase)
        if (evt.ToStatus == "Broderie" && evt.AssignedEmbroidererId.HasValue)
        {
            await _notificationService.CreateAndSendAsync(
                NotificationType.N07_Assigned, evt.OrderId.Value, evt.AssignedEmbroidererId.Value,
                $"Nouvelle commande assignée: {evt.OrderCode}",
                $"Commande {evt.OrderCode} vous a été assignée (broderie).",
                ct: ct);
        }

        if (evt.ToStatus == "Perlage" && evt.AssignedBeaderId.HasValue)
        {
            await _notificationService.CreateAndSendAsync(
                NotificationType.N07_Assigned, evt.OrderId.Value, evt.AssignedBeaderId.Value,
                $"Nouvelle commande assignée: {evt.OrderCode}",
                $"Commande {evt.OrderCode} vous a été assignée (perlage).",
                ct: ct);
        }
    }
}
