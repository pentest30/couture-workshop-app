using Couture.Clients.Persistence;
using Couture.Orders.Contracts;
using Couture.Orders.Contracts.Dtos;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Orders.Features.GetOrder;

public sealed class GetOrderHandler : IQueryHandler<GetOrderQuery, OrderDetailDto?>
{
    private readonly OrdersDbContext _db;
    private readonly ClientsDbContext _clientsDb;

    public GetOrderHandler(OrdersDbContext db, ClientsDbContext clientsDb)
    {
        _db = db;
        _clientsDb = clientsDb;
    }

    public async ValueTask<OrderDetailDto?> Handle(GetOrderQuery query, CancellationToken ct)
    {
        var id = OrderId.From(query.OrderId);
        var order = await _db.Orders
            .Include(o => o.Transitions)
            .Include(o => o.Photos)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null) return null;

        var clientId = Couture.Clients.Contracts.ClientId.From(order.ClientId);
        var client = await _clientsDb.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId, ct);
        var clientName = client?.FullName;

        var transitions = order.Transitions.OrderBy(t => t.TransitionedAt).ToList();
        var timeline = new List<TimelineEntryDto>();

        for (int i = 0; i < transitions.Count; i++)
        {
            var t = transitions[i];
            TimeSpan? duration = i + 1 < transitions.Count
                ? transitions[i + 1].TransitionedAt - t.TransitionedAt
                : DateTimeOffset.UtcNow - t.TransitionedAt;

            timeline.Add(new TimelineEntryDto(
                t.FromStatus?.Name,
                t.ToStatus.Name,
                t.ToStatus.Label,
                t.ToStatus.Color,
                t.Reason,
                t.TransitionedBy,
                t.TransitionedAt,
                duration));
        }

        var isLate = !order.Status.IsTerminal && order.ExpectedDeliveryDate < DateOnly.FromDateTime(DateTime.UtcNow);
        var delayDays = isLate ? (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - order.ExpectedDeliveryDate.DayNumber) : 0;

        return new OrderDetailDto(
            order.Id.Value, order.Code, order.ClientId, clientName,
            order.Status.Name, order.Status.Label, order.Status.Color,
            order.WorkType.Name, order.WorkType.Label,
            order.Description, order.Fabric, order.TechnicalNotes,
            order.EmbroideryStyle, order.ThreadColors, order.Density, order.EmbroideryZone,
            order.BeadType, order.Arrangement, order.AffectedZones,
            order.ReceptionDate, order.ExpectedDeliveryDate, order.ActualDeliveryDate,
            order.TotalPrice, 0, // OutstandingBalance computed when payments module joins
            delayDays, isLate,
            order.AssignedTailorId, order.AssignedEmbroidererId, order.AssignedBeaderId,
            order.HasUnpaidBalance,
            order.CatalogModelId,
            timeline,
            order.Photos.Select(p => new PhotoDto(p.Id.Value, p.FileName, p.StoragePath, p.UploadedAt)).ToList(),
            order.CreatedAt);
    }
}
