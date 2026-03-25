using Xunit;
using FluentAssertions;
using Couture.Notifications.Domain;
using Couture.Notifications.Features.MarkRead;
using Couture.Notifications.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Tests;

public class MarkReadHandlerTests
{
    private static NotificationsDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<NotificationsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task MarkRead_SingleNotification_SetsIsRead()
    {
        var db = CreateDb();
        var userId = Guid.NewGuid();
        var notif = Notification.Create(NotificationType.N01_Overdue, Guid.NewGuid(), userId, "Test", "Msg", false);
        db.Notifications.Add(notif);
        await db.SaveChangesAsync();
        var handler = new MarkReadHandler(db);

        await handler.Handle(new MarkReadCommand(notif.Id.Value, userId), CancellationToken.None);

        var updated = await db.Notifications.FirstAsync();
        updated.IsRead.Should().BeTrue();
        updated.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAllRead_SetsAllAsRead()
    {
        var db = CreateDb();
        var userId = Guid.NewGuid();
        db.Notifications.AddRange(
            Notification.Create(NotificationType.N01_Overdue, Guid.NewGuid(), userId, "T1", "M1", false),
            Notification.Create(NotificationType.N02_DueIn24h, Guid.NewGuid(), userId, "T2", "M2", false));
        await db.SaveChangesAsync();
        var handler = new MarkReadHandler(db);

        await handler.Handle(new MarkAllReadCommand(userId), CancellationToken.None);

        var all = await db.Notifications.ToListAsync();
        all.Should().AllSatisfy(n => n.IsRead.Should().BeTrue());
    }

    [Fact]
    public async Task MarkRead_WrongUser_DoesNothing()
    {
        var db = CreateDb();
        var notif = Notification.Create(NotificationType.N01_Overdue, Guid.NewGuid(), Guid.NewGuid(), "Test", "Msg", false);
        db.Notifications.Add(notif);
        await db.SaveChangesAsync();
        var handler = new MarkReadHandler(db);

        await handler.Handle(new MarkReadCommand(notif.Id.Value, Guid.NewGuid()), CancellationToken.None);

        var unchanged = await db.Notifications.FirstAsync();
        unchanged.IsRead.Should().BeFalse();
    }
}
