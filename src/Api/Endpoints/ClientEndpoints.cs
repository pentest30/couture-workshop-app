using Couture.Clients.Features.CreateClient;
using Couture.Clients.Features.GetClient;
using Couture.Clients.Features.SearchClients;
using Couture.Identity.Contracts;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Couture.Api.Endpoints;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/clients").WithTags("Clients").RequireAuthorization();

        group.MapPost("/", CreateClient)
            .RequireAuthorization(CouturePermissions.ClientsCreate);

        group.MapGet("/search", SearchClients)
            .RequireAuthorization(CouturePermissions.ClientsView);

        group.MapGet("/{id:guid}", GetClient)
            .RequireAuthorization(CouturePermissions.ClientsView);
    }

    private static async Task<IResult> CreateClient(
        [FromBody] CreateClientCommand command,
        IMediator mediator)
    {
        try
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/clients/{result.ClientId}", result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> SearchClients(
        [FromQuery] string q,
        IMediator mediator)
    {
        var result = await mediator.Send(new SearchClientsQuery(q));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetClient(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new GetClientQuery(id));
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}
