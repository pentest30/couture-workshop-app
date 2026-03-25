using Xunit;
using FluentAssertions;
using Couture.Notifications.Domain;
using Couture.Notifications.Features.ListNotifications;
using Couture.Notifications.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Tests;

public class ListNotificationsHandlerTests
{
    private static NotificationsDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<NotificationsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static void SeedNotifications(NotificationsDbContext db, Guid userId)
    {
        db.Notifications.AddRange(
            Notification.Create(NotificationType.N01_Overdue, Guid.NewGuid(), userId, "Retard 1", "Message 1", false),
            Notification.Create(NotificationType.N02_DueIn24h, Guid.NewGuid(), userId, "24h", "Message 2", false),
            Notification.Create(NotificationType.N08_UnpaidDelivery, Guid.NewGuid(), userId, "Impayé", "Message 3", false));
        db.SaveChanges();

        // Mark first as read
        var first = db.Notifications.First();
        first.MarkAsRead();
        db.SaveChanges();
    }

    [Fact]
    public async Task Handle_All_ReturnsAllNotifications()
    {
        var db = CreateDb();
        var userId = Guid.NewGuid();
        SeedNotifications(db, userId);
        var handler = new ListNotificationsHandler(db);

        var result = await handler.Handle(new ListNotificationsQuery(userId, "all"), CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.UnreadCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Unread_ReturnsOnlyUnread()
    {
        var db = CreateDb();
        var userId = Guid.NewGuid();
        SeedNotifications(db, userId);
        var handler = new ListNotificationsHandler(db);

        var result = await handler.Handle(new ListNotificationsQuery(userId, "unread"), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(n => n.IsRead.Should().BeFalse());
    }

    [Fact]
    public async Task Handle_Critical_ReturnsOnlyCritical()
    {
        var db = CreateDb();
        var userId = Guid.NewGuid();
        SeedNotifications(db, userId);
        var handler = new ListNotificationsHandler(db);

        var result = await handler.Handle(new ListNotificationsQuery(userId, "critical"), CancellationToken.None);

        result.Items.Should().HaveCount(2); // N01 (Critical) + N08 (Critical)
    }

    [Fact]
    public async Task Handle_DifferentUser_ReturnsEmpty()
    {
        var db = CreateDb();
        SeedNotifications(db, Guid.NewGuid());
        var handler = new ListNotificationsHandler(db);

        var result = await handler.Handle(new ListNotificationsQuery(Guid.NewGuid(), "all"), CancellationToken.None);

        result.Items.Should().BeEmpty();
    }
}
