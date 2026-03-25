using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Orders.Features.DeactivateOrder;

public sealed record DeactivateOrderCommand(Guid OrderId) : ICommand;

public sealed class DeactivateOrderHandler : ICommandHandler<DeactivateOrderCommand>
{
    private readonly OrdersDbContext _db;
    public DeactivateOrderHandler(OrdersDbContext db) => _db = db;

    public async ValueTask<Unit> Handle(DeactivateOrderCommand cmd, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == Couture.Orders.Contracts.OrderId.From(cmd.OrderId), ct)
            ?? throw new InvalidOperationException("Commande introuvable.");
        order.Deactivate();
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
