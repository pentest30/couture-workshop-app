using Couture.Catalog.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Catalog.Features.RemoveModelPhoto;

public sealed record RemoveModelPhotoCommand(Guid ModelId, Guid PhotoId) : ICommand;

public sealed class RemoveModelPhotoHandler : ICommandHandler<RemoveModelPhotoCommand>
{
    private readonly CatalogDbContext _db;
    public RemoveModelPhotoHandler(CatalogDbContext db) => _db = db;

    public async ValueTask<Unit> Handle(RemoveModelPhotoCommand cmd, CancellationToken ct)
    {
        var model = await _db.Models.Include(m => m.Photos).FirstOrDefaultAsync(m => m.Id == Contracts.ModelId.From(cmd.ModelId), ct)
            ?? throw new InvalidOperationException("Model not found.");
        model.RemovePhoto(cmd.PhotoId);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
