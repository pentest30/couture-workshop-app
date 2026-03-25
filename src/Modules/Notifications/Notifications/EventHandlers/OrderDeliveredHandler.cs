using Couture.Notifications.Domain;
using Couture.Orders.Contracts.Events;
using Mediator;

namespace Couture.Notifications.EventHandlers;

public sealed class OrderDeliveredHandler : INotificationHandler<OrderDeliveredEvent>
{
    private readonly NotificationService _notificationService;

    public OrderDeliveredHandler(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async ValueTask Handle(OrderDeliveredEvent evt, CancellationToken ct)
    {
        // N08: Delivery with unpaid balance
        if (evt.HasUnpaidBalance)
        {
            // Notify manager (we don't know the manager ID here, use a well-known ID or broadcast)
            // For now, log it — the notification will be visible in the notification center
            await _notificationService.CreateAndSendAsync(
                NotificationType.N08_UnpaidDelivery, evt.OrderId.Value, Guid.Empty, // TODO: resolve manager ID
                $"Livraison avec solde impayé: {evt.OrderCode}",
                $"Commande {evt.OrderCode} livrée avec solde impayé. Motif: {evt.UnpaidReason ?? "non spécifié"}.",
                ct: ct);
        }
    }
}
