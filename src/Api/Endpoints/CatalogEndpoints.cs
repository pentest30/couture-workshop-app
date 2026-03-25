using Couture.Api.Pdf;
using Couture.Catalog.Features.AddModelPhoto;
using Couture.Catalog.Features.CreateFabric;
using Couture.Catalog.Features.CreateModel;
using Couture.Catalog.Features.GetFabric;
using Couture.Catalog.Features.GetModel;
using Couture.Catalog.Features.LinkFabric;
using Couture.Catalog.Features.ListFabrics;
using Couture.Catalog.Features.ListModels;
using Couture.Catalog.Features.RemoveModelPhoto;
using Couture.Catalog.Features.UpdateModel;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Couture.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/catalog").WithTags("Catalog").RequireAuthorization();

        // Models
        group.MapGet("/", async (IMediator m, [FromQuery] string? search, [FromQuery] string? category,
            [FromQuery] bool? isPublic, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
            Results.Ok(await m.Send(new ListModelsQuery(search, category, isPublic, page, pageSize))));

        group.MapGet("/{id:guid}", async (Guid id, IMediator m) =>
        {
            var result = await m.Send(new GetModelQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapGet("/{id:guid}/pdf", async (Guid id, IMediator m) =>
        {
            var model = await m.Send(new GetModelQuery(id));
            if (model is null) return Results.NotFound();
            var pdf = CatalogModelPdfGenerator.Generate(new CatalogModelPdfData(
                model.Photos.Count > 0 ? $"MOD-{model.Id.ToString()[..8]}" : $"MOD-{model.Id.ToString()[..8]}",
                model.Name, model.CategoryLabel, model.WorkType,
                model.BasePrice, model.EstimatedDays, model.IsPublic,
                model.Description,
                model.Fabrics.Select(f => new CatalogFabricPdfItem(f.Name, f.Type, f.PricePerMeter)).ToList()));
            return Results.File(pdf, "application/pdf", $"Modele-{model.Name}.pdf");
        }).AllowAnonymous();

        group.MapPost("/", async ([FromBody] CreateModelCommand cmd, IMediator m) =>
        {
            try
            {
                var result = await m.Send(cmd);
                return Results.Created($"/api/catalog/{result.Id}", result);
            }
            catch (Exception ex) { return Results.BadRequest(new { error = ex.Message }); }
        });

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateModelRequest req, IMediator m) =>
        {
            try
            {
                await m.Send(new UpdateModelCommand(id, req.Name, req.Category, req.WorkType, req.BasePrice, req.EstimatedDays, req.IsPublic, req.Description));
                return Results.NoContent();
            }
            catch (Exception ex) { return Results.BadRequest(new { error = ex.Message }); }
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator m) =>
        {
            try { await m.Send(new Couture.Catalog.Features.DeactivateModel.DeactivateModelCommand(id)); return Results.NoContent(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        });

        // Model photos
        group.MapPost("/{id:guid}/photos", async (Guid id, [FromBody] AddPhotoRequest req, IMediator m) =>
        {
            var photoId = await m.Send(new AddModelPhotoCommand(id, req.FileName, req.StoragePath, req.SortOrder));
            return Results.Created($"/api/catalog/{id}/photos/{photoId}", new { photoId });
        });

        group.MapDelete("/{id:guid}/photos/{photoId:guid}", async (Guid id, Guid photoId, IMediator m) =>
        {
            await m.Send(new RemoveModelPhotoCommand(id, photoId));
            return Results.NoContent();
        });

        // Model-Fabric links
        group.MapPost("/{id:guid}/fabrics/{fabricId:guid}", async (Guid id, Guid fabricId, IMediator m) =>
        {
            await m.Send(new LinkFabricCommand(id, fabricId));
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}/fabrics/{fabricId:guid}", async (Guid id, Guid fabricId, IMediator m) =>
        {
            await m.Send(new UnlinkFabricCommand(id, fabricId));
            return Results.NoContent();
        });

        // Fabrics
        group.MapGet("/fabrics", async (IMediator m, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
            Results.Ok(await m.Send(new ListFabricsQuery(search, page, pageSize))));

        group.MapGet("/fabrics/{id:guid}", async (Guid id, IMediator m) =>
        {
            var result = await m.Send(new GetFabricQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPost("/fabrics", async ([FromBody] CreateFabricCommand cmd, IMediator m) =>
        {
            try
            {
                var id = await m.Send(cmd);
                return Results.Created($"/api/catalog/fabrics/{id}", new { id });
            }
            catch (Exception ex) { return Results.BadRequest(new { error = ex.Message }); }
        });
    }
}

public record UpdateModelRequest(string? Name, string? Category, string? WorkType, decimal? BasePrice, int? EstimatedDays, bool? IsPublic, string? Description);
public record AddPhotoRequest(string FileName, string StoragePath, int SortOrder);
