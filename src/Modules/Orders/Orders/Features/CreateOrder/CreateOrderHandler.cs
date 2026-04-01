using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Orders.Features.CreateOrder;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly OrdersDbContext _db;

    public CreateOrderHandler(OrdersDbContext db)
    {
        _db = db;
    }

    public async ValueTask<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var workType = WorkType.FromName(command.WorkType, ignoreCase: true);

        // Generate sequential code — include soft-deleted orders to avoid duplicate codes
        var year = DateTime.UtcNow.Year;
        var count = _db.Orders.IgnoreQueryFilters().Count(o => o.ReceptionDate.Year == year) + 1;
        var code = $"CMD-{year}-{count:D4}";

        var order = Order.Create(
            code: code,
            clientId: command.ClientId,
            workType: workType,
            expectedDeliveryDate: command.ExpectedDeliveryDate,
            totalPrice: command.TotalPrice,
            description: command.Description,
            fabric: command.Fabric,
            technicalNotes: command.TechnicalNotes,
            assignedTailorId: command.AssignedTailorId,
            assignedEmbroidererId: command.AssignedEmbroidererId,
            assignedBeaderId: command.AssignedBeaderId,
            catalogModelId: command.CatalogModelId);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        var outstandingBalance = command.TotalPrice - (command.InitialDeposit ?? 0);

        return new CreateOrderResult(order.Id.Value, order.Code, order.Status.Name, outstandingBalance);
    }
}
