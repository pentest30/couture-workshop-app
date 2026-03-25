using Couture.Catalog.Domain;
using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.CreateModel;

public sealed record CreateModelCommand(string Name, string Category, string WorkType,
    decimal BasePrice, int EstimatedDays, bool IsPublic, string? Description) : ICommand<CreateModelResult>;

public sealed record CreateModelResult(Guid Id, string Code, string Name);

public sealed class CreateModelHandler : ICommandHandler<CreateModelCommand, CreateModelResult>
{
    private readonly CatalogDbContext _db;
    public CreateModelHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<CreateModelResult> Handle(CreateModelCommand cmd, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _db.Models.CountAsync(m => m.Code.StartsWith($"MOD-{year}"), ct) + 1;
        var code = $"MOD-{year}-{count:D4}";

        var category = ModelCategory.FromName(cmd.Category, ignoreCase: true);
        var model = Model.Create(code, cmd.Name, category, cmd.WorkType, cmd.BasePrice, cmd.EstimatedDays, cmd.IsPublic, cmd.Description);
        _db.Models.Add(model);
        await _db.SaveChangesAsync(ct);
        return new CreateModelResult(model.Id.Value, model.Code, model.Name);
    }
}
