using Couture.Notifications.Domain;
using Couture.Orders.Contracts.Events;
using Mediator;

namespace Couture.Notifications.EventHandlers;

public sealed class StatusChangedHandler : INotificationHandler<StatusChangedEvent>
{
    private readonly NotificationService _notificationService;

    public StatusChangedHandler(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async ValueTask Handle(StatusChangedEvent evt, CancellationToken ct)
    {
        // N05: Status -> Retouche
        if (evt.ToStatus == "Retouche")
        {
            await _notificationService.CreateAndSendToManagersAsync(
                NotificationType.N05_Retouche, evt.OrderId.Value,
                $"Commande {evt.OrderCode} en retouche",
                $"Commande {evt.OrderCode} passée en retouche. Motif: {evt.Reason ?? "non spécifié"}.",
                ct: ct);
        }

        // N06: Status -> Prete
        if (evt.ToStatus == "Prete")
        {
            await _notificationService.CreateAndSendToManagersAsync(
                NotificationType.N06_Ready, evt.OrderId.Value,
                $"Commande {evt.OrderCode} prête",
                $"Commande {evt.OrderCode} est prête pour la livraison.",
                ct: ct);
        }
    }
}
