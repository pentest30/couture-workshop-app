using Couture.Clients.Features.CreateClient;
using DuplicatePhoneException = Couture.Clients.Features.CreateClient.DuplicatePhoneException;
using Couture.Clients.Features.GetClient;
using Couture.Clients.Features.GetMeasurementHistory;
using Couture.Clients.Features.ListClients;
using Couture.Clients.Features.ManageMeasurementFields;
using Couture.Clients.Features.RecordMeasurements;
using Couture.Clients.Features.SearchClients;
using Couture.Clients.Features.UpdateClient;
using Couture.Identity.Contracts;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Couture.Api.Endpoints;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/clients").WithTags("Clients").RequireAuthorization();

        group.MapPost("/", CreateClient).RequireAuthorization(CouturePermissions.ClientsCreate);
        group.MapGet("/", ListClients).RequireAuthorization(CouturePermissions.ClientsView);
        group.MapGet("/search", SearchClients).RequireAuthorization(CouturePermissions.ClientsView);
        group.MapGet("/{id:guid}", GetClient).RequireAuthorization(CouturePermissions.ClientsView);
        group.MapPut("/{id:guid}", UpdateClient).RequireAuthorization(CouturePermissions.ClientsUpdate);
        group.MapGet("/{id:guid}/measurements", GetMeasurementHistory).RequireAuthorization(CouturePermissions.ClientsView);
        group.MapPost("/{id:guid}/measurements", RecordMeasurements).RequireAuthorization(CouturePermissions.ClientsCreate);
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            try { await mediator.Send(new Couture.Clients.Features.DeactivateClient.DeactivateClientCommand(id)); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization(CouturePermissions.ClientsUpdate);

        var fields = app.MapGroup("/api/measurement-fields").WithTags("Measurement Fields")
            .RequireAuthorization(CouturePermissions.SettingsManage);
        fields.MapPost("/", CreateMeasurementField);
        fields.MapDelete("/{fieldId:guid}", DeleteMeasurementField);
    }

    private static async Task<IResult> CreateClient([FromBody] CreateClientCommand command, IMediator mediator)
    {
        try
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/clients/{result.ClientId}", result);
        }
        catch (DuplicatePhoneException ex)
        {
            return Results.Conflict(new
            {
                error = ex.Message,
                duplicate = true,
                existingClientId = ex.ExistingClientId,
                existingCode = ex.ExistingCode,
                existingName = ex.ExistingName,
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
    private static async Task<IResult> ListClients(IMediator mediator, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Results.Ok(await mediator.Send(new ListClientsQuery(search, page, pageSize)));
    private static async Task<IResult> SearchClients([FromQuery] string q, IMediator mediator)
        => Results.Ok(await mediator.Send(new SearchClientsQuery(q)));
    private static async Task<IResult> GetClient(Guid id, IMediator mediator)
    { var r = await mediator.Send(new GetClientQuery(id)); return r is null ? Results.NotFound() : Results.Ok(r); }
    private static async Task<IResult> UpdateClient(Guid id, [FromBody] UpdateClientRequest req, IMediator mediator)
    {
        try { await mediator.Send(new UpdateClientCommand(id, req.FirstName, req.LastName, req.PrimaryPhone, req.SecondaryPhone, req.Address, req.DateOfBirth, req.Notes)); return Results.NoContent(); }
        catch (InvalidOperationException ex) { return Results.NotFound(new { error = ex.Message }); }
    }
    private static async Task<IResult> GetMeasurementHistory(Guid id, IMediator mediator)
        => Results.Ok(await mediator.Send(new GetMeasurementHistoryQuery(id)));
    private static async Task<IResult> RecordMeasurements(Guid id, [FromBody] RecordMeasurementsRequest req, IMediator mediator, ICurrentUser user)
    {
        await mediator.Send(new RecordMeasurementsCommand(id, req.Measurements.Select(m => new MeasurementEntry(m.FieldId, m.Value)).ToList(), user.UserId));
        return Results.NoContent();
    }
    private static async Task<IResult> CreateMeasurementField([FromBody] CreateFieldRequest req, IMediator mediator)
    { var id = await mediator.Send(new CreateMeasurementFieldCommand(req.Name, req.Unit, req.DisplayOrder)); return Results.Created($"/api/measurement-fields/{id}", new { id }); }
    private static async Task<IResult> DeleteMeasurementField(Guid fieldId, IMediator mediator)
    { await mediator.Send(new DeleteMeasurementFieldCommand(fieldId)); return Results.NoContent(); }
}

public record UpdateClientRequest(string? FirstName, string? LastName, string? PrimaryPhone, string? SecondaryPhone, string? Address, DateOnly? DateOfBirth, string? Notes);
public record RecordMeasurementsRequest(List<MeasurementEntryDto> Measurements);
public record MeasurementEntryDto(Guid FieldId, decimal Value);
public record CreateFieldRequest(string Name, string Unit, int DisplayOrder);
