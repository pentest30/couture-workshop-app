using Xunit;
using FluentAssertions;
using Couture.Notifications.Domain;
using Couture.Notifications.Jobs;
using Couture.Notifications.Sms;
using Couture.Orders.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
namespace Couture.Notifications.Tests;

public class EvaluateStalledOrdersJobTests
{
    private EvaluateStalledOrdersJob CreateJob(
        Couture.Notifications.Persistence.NotificationsDbContext notifDb,
        Couture.Orders.Persistence.OrdersDbContext ordersDb)
    {
        var sms = new MockSmsGateway(NullLogger<MockSmsGateway>.Instance);
        var service = new NotificationService(notifDb, sms, new NullHubContext(), new NullManagerResolver(), NullLogger<NotificationService>.Instance);
        return new EvaluateStalledOrdersJob(ordersDb, notifDb, service, NullLogger<EvaluateStalledOrdersJob>.Instance);
    }

    /// <summary>
    /// Creates an order in a given status and backdates its last transition to simulate days-in-status.
    /// </summary>
    private static Order SeedStalledOrder(
        Couture.Orders.Persistence.OrdersDbContext db,
        OrderStatus targetStatus,
        WorkType workType,
        int daysInStatus,
        string code = "CMD-2026-0001")
    {
        var order = Order.Create(code, Guid.NewGuid(), workType,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), 15000m);
        order.Update(assignedTailorId: Guid.NewGuid());

        // Move to target status via valid transitions
        if (targetStatus != OrderStatus.Recue)
            order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid(), null);
        if (targetStatus == OrderStatus.Broderie)
            order.ChangeStatus(OrderStatus.Broderie, Guid.NewGuid(), null);
        else if (targetStatus == OrderStatus.Perlage)
        {
            if (workType == WorkType.Mixte)
                order.ChangeStatus(OrderStatus.Broderie, Guid.NewGuid(), null);
            order.ChangeStatus(OrderStatus.Perlage, Guid.NewGuid(), null);
        }
        else if (targetStatus == OrderStatus.Retouche)
            order.ChangeStatus(OrderStatus.Retouche, Guid.NewGuid(), "Test retouche");
        else if (targetStatus == OrderStatus.Prete)
            order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid(), null);

        // Backdate ALL transitions to simulate time in status
        foreach (var t in order.Transitions)
            t.TransitionedAt = DateTimeOffset.UtcNow.AddDays(-daysInStatus);

        db.Orders.Add(order);
        db.SaveChanges();
        db.ChangeTracker.Clear();
        return order;
    }

    [Fact]
    public async Task OrderStalled8Days_ShouldCreateNotification()
    {
        var (notifDb, ordersDb) = TestDbHelper.Create();
        SeedStalledOrder(ordersDb, OrderStatus.EnCours, WorkType.Simple, daysInStatus: 8);

        // Verify the backdating worked - check both DbSet and navigation
        var directTransition = ordersDb.StatusTransitions.OrderByDescending(t => t.TransitionedAt).First();
        var directDays = (int)(DateTimeOffset.UtcNow - directTransition.TransitionedAt).TotalDays;

        var verifyOrder = ordersDb.Orders.Include(o => o.Transitions).First();
        var lastT = verifyOrder.Transitions.OrderByDescending(t => t.TransitionedAt).First();
        var daysAgo = (int)(DateTimeOffset.UtcNow - lastT.TransitionedAt).TotalDays;

        // If direct query also shows 0, InMemory didn't persist the backdate
        daysAgo.Should().BeGreaterThanOrEqualTo(7, $"transition should have been backdated (direct: {directDays}d, nav: {daysAgo}d, value: {lastT.TransitionedAt})");

        var job = CreateJob(notifDb, ordersDb);
        await job.ExecuteAsync();

        notifDb.Notifications.Should().HaveCountGreaterThan(0);
        var notif = notifDb.Notifications.First();
        notif.Type.Should().Be(NotificationType.N04_Stalled);
        notif.Title.Should().Contain("CMD-2026-0001");
    }

    [Fact]
    public async Task OrderStalled5Days_ShouldNotTrigger()
    {
        var (notifDb, ordersDb) = TestDbHelper.Create();
        SeedStalledOrder(ordersDb, OrderStatus.EnCours, WorkType.Simple, daysInStatus: 5);

        var job = CreateJob(notifDb, ordersDb);
        await job.ExecuteAsync();

        notifDb.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task OrderStalledExactly7Days_ShouldNotTrigger()
    {
        var (notifDb, ordersDb) = TestDbHelper.Create();
        SeedStalledOrder(ordersDb, OrderStatus.EnCours, WorkType.Brode, daysInStatus: 7);

        var job = CreateJob(notifDb, ordersDb);
        await job.ExecuteAsync();

        // threshold is 7, condition is > 7, so exactly 7 should NOT trigger
        notifDb.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task AllWorkTypes_UseSame7DayThreshold()
    {
        var (notifDb, ordersDb) = TestDbHelper.Create();

        SeedStalledOrder(ordersDb, OrderStatus.EnCours, WorkType.Simple, daysInStatus: 10, code: "CMD-2026-0001");
        SeedStalledOrder(ordersDb, OrderStatus.EnCours, WorkType.Brode, daysInStatus: 10, code: "CMD-2026-0002");
        SeedStalledOrder(ordersDb, OrderStatus.EnCours, WorkType.Perle, daysInStatus: 10, code: "CMD-2026-0003");
        SeedStalledOrder(ordersDb, OrderStatus.EnCours, WorkType.Mixte, daysInStatus: 10, code: "CMD-2026-0004");

        var job = CreateJob(notifDb, ordersDb);
        await job.ExecuteAsync();

        // Each order should trigger at least 1 notification (to manager)
        notifDb.Notifications.Count().Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task DeliveredOrder_IsIgnored()
    {
        var (notifDb, ordersDb) = TestDbHelper.Create();
        var order = Order.Create("CMD-2026-0099", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), 15000m);
        order.Update(assignedTailorId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid(), null);
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid(), null);
        order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 0, null);
        ordersDb.Orders.Add(order);
        ordersDb.SaveChanges();

        var job = CreateJob(notifDb, ordersDb);
        await job.ExecuteAsync();

        notifDb.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task RecueStatus_IsIgnored()
    {
        var (notifDb, ordersDb) = TestDbHelper.Create();
        var order = Order.Create("CMD-2026-0098", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), 15000m);
        ordersDb.Orders.Add(order);
        ordersDb.SaveChanges();

        var job = CreateJob(notifDb, ordersDb);
        await job.ExecuteAsync();

        notifDb.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task DisabledConfig_SkipsAll()
    {
        var (notifDb, ordersDb) = TestDbHelper.Create();
        SeedStalledOrder(ordersDb, OrderStatus.EnCours, WorkType.Simple, daysInStatus: 15);

        var config = NotificationConfig.Create(NotificationType.N04_Stalled);
        config.Update(isEnabled: false);
        notifDb.NotificationConfigs.Add(config);
        await notifDb.SaveChangesAsync();

        var job = CreateJob(notifDb, ordersDb);
        await job.ExecuteAsync();

        notifDb.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task DuplicateRun_DoesNotCreateDuplicates()
    {
        var (notifDb, ordersDb) = TestDbHelper.Create();
        SeedStalledOrder(ordersDb, OrderStatus.EnCours, WorkType.Simple, daysInStatus: 10);

        var job = CreateJob(notifDb, ordersDb);
        await job.ExecuteAsync();
        var countAfterFirst = notifDb.Notifications.Count();
        countAfterFirst.Should().BeGreaterThan(0);

        // Run again — deduplication should prevent duplicates
        await job.ExecuteAsync();
        notifDb.Notifications.Count().Should().Be(countAfterFirst);
    }
}
