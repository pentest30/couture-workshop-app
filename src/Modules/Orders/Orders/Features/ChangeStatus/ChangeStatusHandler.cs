using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Orders.Features.ChangeStatus;

public sealed class ChangeStatusHandler : ICommandHandler<ChangeStatusCommand, ChangeStatusResult>
{
    private readonly OrdersDbContext _db;

    public ChangeStatusHandler(OrdersDbContext db) => _db = db;

    public async ValueTask<ChangeStatusResult> Handle(ChangeStatusCommand command, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.Transitions)
            .FirstOrDefaultAsync(o => o.Id.Value == command.OrderId, ct)
            ?? throw new InvalidOperationException("Order not found.");

        var newStatus = OrderStatus.FromName(command.NewStatus, ignoreCase: true);
        var previousStatus = order.Status.Name;

        // Update artisan assignments if provided
        if (command.AssignedEmbroidererId.HasValue)
            order.Update(assignedEmbroidererId: command.AssignedEmbroidererId);
        if (command.AssignedBeaderId.HasValue)
            order.Update(assignedBeaderId: command.AssignedBeaderId);

        if (newStatus == OrderStatus.Livree)
        {
            order.MarkAsDelivered(
                command.ChangedByUserId,
                command.ActualDeliveryDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                0, // TODO: compute from payments
                command.Reason);
        }
        else
        {
            order.ChangeStatus(newStatus, command.ChangedByUserId, command.Reason);
        }

        await _db.SaveChangesAsync(ct);

        return new ChangeStatusResult(order.Id.Value, previousStatus, order.Status.Name, DateTimeOffset.UtcNow);
    }
}
