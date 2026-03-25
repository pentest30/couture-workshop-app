using Couture.Catalog.Domain;
using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.UpdateModel;

public sealed record UpdateModelCommand(Guid Id, string? Name, string? Category, string? WorkType,
    decimal? BasePrice, int? EstimatedDays, bool? IsPublic, string? Description) : ICommand;

public sealed class UpdateModelHandler : ICommandHandler<UpdateModelCommand>
{
    private readonly CatalogDbContext _db;
    public UpdateModelHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<Unit> Handle(UpdateModelCommand cmd, CancellationToken ct)
    {
        var model = await _db.Models.FirstOrDefaultAsync(m => m.Id == Contracts.ModelId.From(cmd.Id), ct)
            ?? throw new InvalidOperationException("Model not found.");
        var cat = cmd.Category is not null ? ModelCategory.FromName(cmd.Category, ignoreCase: true) : null;
        model.Update(cmd.Name, cat, cmd.WorkType, cmd.BasePrice, cmd.EstimatedDays, cmd.IsPublic, cmd.Description);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
