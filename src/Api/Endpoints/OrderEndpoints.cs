using Couture.Identity.Contracts;
using Couture.Orders.Features.ChangeStatus;
using Couture.Orders.Features.CreateOrder;
using Couture.Orders.Features.GetOrder;
using Couture.Orders.Features.ListOrders;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Couture.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization();

        group.MapPost("/", CreateOrder)
            .RequireAuthorization(CouturePermissions.OrdersCreate);

        group.MapGet("/", ListOrders)
            .RequireAuthorization(CouturePermissions.OrdersView);

        group.MapGet("/{id:guid}", GetOrder)
            .RequireAuthorization(CouturePermissions.OrdersView);

        group.MapPost("/{id:guid}/status", ChangeStatus)
            .RequireAuthorization(CouturePermissions.OrdersChangeStatus);
    }

    private static async Task<IResult> CreateOrder(
        [FromBody] CreateOrderCommand command,
        IMediator mediator)
    {
        var result = await mediator.Send(command);
        return Results.Created($"/api/orders/{result.OrderId}", result);
    }

    private static async Task<IResult> ListOrders(
        IMediator mediator,
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
        [FromQuery] string sortDir = "desc")
    {
        var query = new ListOrdersQuery(search, status, workType, artisanId, dateFrom, dateTo, lateOnly, page, pageSize, sortBy, sortDir);
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
        var command = new ChangeStatusCommand(
            id, request.NewStatus, request.Reason,
            request.AssignedEmbroidererId, request.AssignedBeaderId,
            request.ActualDeliveryDate, currentUser.UserId);
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }
}

public record ChangeStatusRequest(string NewStatus, string? Reason, Guid? AssignedEmbroidererId, Guid? AssignedBeaderId, DateOnly? ActualDeliveryDate);
