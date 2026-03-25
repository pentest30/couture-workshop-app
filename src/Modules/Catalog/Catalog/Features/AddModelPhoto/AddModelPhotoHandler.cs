using Couture.Catalog.Domain;
using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.AddModelPhoto;

public sealed record AddModelPhotoCommand(Guid ModelId, string FileName, string StoragePath, int SortOrder) : ICommand<Guid>;

public sealed class AddModelPhotoHandler : ICommandHandler<AddModelPhotoCommand, Guid>
{
    private readonly CatalogDbContext _db;
    public AddModelPhotoHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<Guid> Handle(AddModelPhotoCommand cmd, CancellationToken ct)
    {
        var modelId = Contracts.ModelId.From(cmd.ModelId);
        var exists = await _db.Models.AnyAsync(m => m.Id == modelId, ct);
        if (!exists) throw new InvalidOperationException("Model not found.");

        var photo = ModelPhoto.Create(modelId, cmd.FileName, cmd.StoragePath, cmd.SortOrder);
        _db.ModelPhotos.Add(photo);
        await _db.SaveChangesAsync(ct);
        return photo.Id;
    }
}
