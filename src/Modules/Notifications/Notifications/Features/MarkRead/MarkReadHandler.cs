using Couture.Notifications.Contracts;
using Couture.Notifications.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Features.MarkRead;

public sealed class MarkReadHandler : ICommandHandler<MarkReadCommand>, ICommandHandler<MarkAllReadCommand>
{
    private readonly NotificationsDbContext _db;

    public MarkReadHandler(NotificationsDbContext db) => _db = db;

    public async ValueTask<Unit> Handle(MarkReadCommand command, CancellationToken ct)
    {
        var id = NotificationId.From(command.NotificationId);
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.RecipientId == command.UserId, ct);

        if (notification is not null)
        {
            notification.MarkAsRead();
            await _db.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }

    public async ValueTask<Unit> Handle(MarkAllReadCommand command, CancellationToken ct)
    {
        var unread = await _db.Notifications
            .Where(n => n.RecipientId == command.UserId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in unread)
            n.MarkAsRead();

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
