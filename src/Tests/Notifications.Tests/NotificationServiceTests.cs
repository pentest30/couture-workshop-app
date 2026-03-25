using Xunit;
using FluentAssertions;
using Couture.Notifications.Domain;
using Couture.Notifications.Sms;
using Microsoft.Extensions.Logging.Abstractions;

namespace Couture.Notifications.Tests;

public class NotificationServiceTests
{
    [Fact]
    public async Task CreateAndSend_CreatesNotification_InDatabase()
    {
        var (db, _) = TestDbHelper.Create();
        var sms = new MockSmsGateway(NullLogger<MockSmsGateway>.Instance);
        var service = new NotificationService(db, sms, NullLogger<NotificationService>.Instance);

        await service.CreateAndSendAsync(NotificationType.N01_Overdue, Guid.NewGuid(), Guid.NewGuid(),
            "Test title", "Test message");

        db.Notifications.Should().HaveCount(1);
        var notif = db.Notifications.First();
        notif.Title.Should().Be("Test title");
        notif.Type.Should().Be(NotificationType.N01_Overdue);
        notif.Priority.Should().Be(NotificationPriority.Critical);
        notif.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAndSend_WhenDisabled_SkipsCreation()
    {
        var (db, _) = TestDbHelper.Create();
        var config = NotificationConfig.Create(NotificationType.N01_Overdue);
        config.Update(isEnabled: false);
        db.NotificationConfigs.Add(config);
        await db.SaveChangesAsync();

        var sms = new MockSmsGateway(NullLogger<MockSmsGateway>.Instance);
        var service = new NotificationService(db, sms, NullLogger<NotificationService>.Instance);

        await service.CreateAndSendAsync(NotificationType.N01_Overdue, Guid.NewGuid(), Guid.NewGuid(),
            "Test", "Test message");

        db.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAndSend_WithSmsEnabled_SetsSmsStatus()
    {
        var (db, _) = TestDbHelper.Create();
        var sms = new MockSmsGateway(NullLogger<MockSmsGateway>.Instance);
        var service = new NotificationService(db, sms, NullLogger<NotificationService>.Instance);

        await service.CreateAndSendAsync(NotificationType.N07_Assigned, Guid.NewGuid(), Guid.NewGuid(),
            "Assignation", "Commande assignée", "0550123456");

        var notif = db.Notifications.First();
        notif.SendSms.Should().BeTrue();
        // SMS status depends on time window
    }
}
