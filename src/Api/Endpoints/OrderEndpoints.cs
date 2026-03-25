using Couture.Api.Pdf;
using Couture.Catalog.Persistence;
using Couture.Clients.Persistence;
using Couture.Finance.Persistence;
using Couture.Identity.Contracts;
using Couture.Orders.Features.ChangeStatus;
using Couture.Orders.Features.CreateOrder;
using Couture.Orders.Features.GetOrder;
using Couture.Orders.Features.ListOrders;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Couture.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization();

        group.MapPost("/", CreateOrder)
            .RequireAuthorization(CouturePermissions.OrdersCreate);

        group.MapGet("/", ListOrders)
            .RequireAuthorization(p => p.RequireAssertion(ctx => true));

        group.MapGet("/{id:guid}", GetOrder)
            .RequireAuthorization(CouturePermissions.OrdersView);

        group.MapPost("/{id:guid}/status", ChangeStatus)
            .RequireAuthorization(CouturePermissions.OrdersChangeStatus);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try { await mediator.Send(new Couture.Orders.Features.DeactivateOrder.DeactivateOrderCommand(id)); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization(CouturePermissions.OrdersCreate);

        group.MapGet("/{id:guid}/pdf", DownloadOrderPdf).AllowAnonymous();
        group.MapGet("/{id:guid}/worksheet", DownloadWorkSheet).AllowAnonymous();
    }

    private static async Task<IResult> CreateOrder(
        [FromBody] CreateOrderCommand command,
        IMediator mediator)
    {
        try
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/orders/{result.OrderId}", result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListOrders(
        IMediator mediator,
        ICurrentUser currentUser,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? workType,
        [FromQuery] Guid? artisanId,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] bool? lateOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDir = "desc",
        [FromQuery] Guid? clientId = null)
    {
        var viewOwnOnly = !currentUser.HasPermission(CouturePermissions.OrdersView) && currentUser.HasPermission(CouturePermissions.OrdersViewOwn);
        var query = new ListOrdersQuery(search, status, workType, artisanId, dateFrom, dateTo, lateOnly, page, pageSize, sortBy, sortDir, clientId, currentUser.UserId, viewOwnOnly);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetOrder(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new GetOrderQuery(id));
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeStatusRequest request,
        IMediator mediator,
        ICurrentUser currentUser)
    {
        try
        {
            var command = new ChangeStatusCommand(
                id, request.NewStatus, request.Reason,
                request.AssignedEmbroidererId, request.AssignedBeaderId,
                request.ActualDeliveryDate, currentUser.UserId);
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DownloadOrderPdf(
        Guid id, OrdersDbContext ordersDb, ClientsDbContext clientsDb, FinanceDbContext financeDb)
    {
        var order = await ordersDb.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == Couture.Orders.Contracts.OrderId.From(id));
        if (order is null) return Results.NotFound();

        var client = await clientsDb.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Couture.Clients.Contracts.ClientId.From(order.ClientId));
        var totalPaid = await financeDb.Payments.Where(p => p.OrderId == id).SumAsync(p => p.Amount);

        var data = new OrderPdfData(
            order.Code,
            client is not null ? $"{client.FirstName} {client.LastName}" : "—",
            order.Status.Label, order.WorkType.Label,
            order.ReceptionDate.ToString("dd/MM/yyyy"),
            order.ExpectedDeliveryDate.ToString("dd/MM/yyyy"),
            order.ActualDeliveryDate?.ToString("dd/MM/yyyy"),
            order.Description, order.Fabric, order.TechnicalNotes,
            order.EmbroideryStyle, order.BeadType,
            order.TotalPrice, totalPaid, order.TotalPrice - totalPaid);

        var pdf = OrderPdfGenerator.Generate(data);
        return Results.File(pdf, "application/pdf", $"{order.Code}.pdf");
    }

    private static async Task<IResult> DownloadWorkSheet(
        Guid id, OrdersDbContext ordersDb, ClientsDbContext clientsDb, CatalogDbContext catalogDb)
    {
        var order = await ordersDb.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == Couture.Orders.Contracts.OrderId.From(id));
        if (order is null) return Results.NotFound();

        var client = await clientsDb.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Couture.Clients.Contracts.ClientId.From(order.ClientId));

        // Get client measurements
        var measurements = new List<MeasurementItem>();
        if (client is not null)
        {
            var fields = await clientsDb.MeasurementFields.AsNoTracking().ToListAsync();
            var clientMeas = await clientsDb.ClientMeasurements
                .AsNoTracking()
                .Where(cm => cm.ClientId == client.Id)
                .ToListAsync();
            var fieldMap = fields.ToDictionary(f => f.Id, f => f);
            measurements = clientMeas
                .Where(cm => fieldMap.ContainsKey(cm.MeasurementFieldId))
                .GroupBy(cm => cm.MeasurementFieldId)
                .Select(g => g.OrderByDescending(cm => cm.MeasuredAt).First())
                .Select(cm => { var f = fieldMap[cm.MeasurementFieldId]; return new MeasurementItem(f.Name, cm.Value, f.Unit); })
                .ToList();
        }

        // Get catalog model if linked
        string? modelCode = null, modelName = null, modelDesc = null;
        var catalogFabrics = new List<string>();
        if (order.CatalogModelId.HasValue)
        {
            var model = await catalogDb.Models.AsNoTracking()
                .Include(m => m.ModelFabrics)
                .FirstOrDefaultAsync(m => m.Id == Couture.Catalog.Contracts.ModelId.From(order.CatalogModelId.Value));
            if (model is not null)
            {
                modelCode = model.Code;
                modelName = model.Name;
                modelDesc = model.Description;
                var fabricIds = model.ModelFabrics.Select(mf => mf.FabricId).ToList();
                var fabrics = await catalogDb.Fabrics.AsNoTracking()
                    .Where(f => fabricIds.Contains(f.Id)).ToListAsync();
                catalogFabrics = fabrics.Select(f => $"{f.Name} ({f.Type}, {f.Color})").ToList();
            }
        }

        var data = new WorkSheetData(
            order.Code,
            client is not null ? client.FullName : "—",
            client?.Code ?? "—",
            client?.PrimaryPhone ?? "—",
            client?.Address,
            order.WorkType.Label, order.Status.Label,
            order.ReceptionDate.ToString("dd/MM/yyyy"),
            order.ExpectedDeliveryDate.ToString("dd/MM/yyyy"),
            order.TotalPrice,
            order.Description, order.Fabric, order.TechnicalNotes,
            order.EmbroideryStyle, order.ThreadColors, order.Density, order.EmbroideryZone,
            order.BeadType, order.Arrangement, order.AffectedZones,
            modelCode, modelName, modelDesc,
            measurements, catalogFabrics);

        var pdf = WorkSheetPdfGenerator.Generate(data);
        return Results.File(pdf, "application/pdf", $"Fiche-{order.Code}.pdf");
    }
}

public record ChangeStatusRequest(string NewStatus, string? Reason, Guid? AssignedEmbroidererId, Guid? AssignedBeaderId, DateOnly? ActualDeliveryDate);
