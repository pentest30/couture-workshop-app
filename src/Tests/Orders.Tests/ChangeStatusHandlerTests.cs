using Xunit;
using Couture.Orders.Domain;
using Couture.Orders.Features.ChangeStatus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Couture.Orders.Tests;

public class ChangeStatusHandlerTests
{
    private static Order SeedOrder(Couture.Orders.Persistence.OrdersDbContext db, WorkType? workType = null)
    {
        var order = Order.Create("CMD-2026-0001", Guid.NewGuid(), workType ?? WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 15000m);
        order.Update(assignedTailorId: Guid.NewGuid());
        db.Orders.Add(order);
        db.SaveChanges();
        return order;
    }

    [Fact]
    public async Task Handle_ValidTransition_UpdatesStatus()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var order = SeedOrder(db);
        var handler = new ChangeStatusHandler(db);

        var result = await handler.Handle(
            new ChangeStatusCommand(order.Id.Value, "EnCours", null, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.PreviousStatus.Should().Be("Recue");
        result.NewStatus.Should().Be("EnCours");

        var updated = await db.Orders.Include(o => o.Transitions).FirstAsync();
        updated.Status.Should().Be(OrderStatus.EnCours);
        updated.Transitions.Should().HaveCount(2); // initial + transition
    }

    [Fact]
    public async Task Handle_InvalidTransition_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var order = SeedOrder(db);
        var handler = new ChangeStatusHandler(db);

        // Recue -> Prete is invalid
        var act = async () => await handler.Handle(
            new ChangeStatusCommand(order.Id.Value, "Prete", null, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot transition*");
    }

    [Fact]
    public async Task Handle_OrderNotFound_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new ChangeStatusHandler(db);

        var act = async () => await handler.Handle(
            new ChangeStatusCommand(Guid.NewGuid(), "EnCours", null, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ToRetouche_WithoutReason_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var order = SeedOrder(db);
        var handler = new ChangeStatusHandler(db);

        // First move to EnCours
        await handler.Handle(
            new ChangeStatusCommand(order.Id.Value, "EnCours", null, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        // Then try Retouche without reason
        var act = async () => await handler.Handle(
            new ChangeStatusCommand(order.Id.Value, "Retouche", null, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*reason*");
    }

    [Fact]
    public async Task Handle_ToBroderie_AssignsEmbroiderer()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var order = SeedOrder(db, WorkType.Brode);
        var handler = new ChangeStatusHandler(db);
        var embroidererId = Guid.NewGuid();

        await handler.Handle(
            new ChangeStatusCommand(order.Id.Value, "EnCours", null, null, null, null, Guid.NewGuid()),
            CancellationToken.None);

        var result = await handler.Handle(
            new ChangeStatusCommand(order.Id.Value, "Broderie", null, embroidererId, null, null, Guid.NewGuid()),
            CancellationToken.None);

        result.NewStatus.Should().Be("Broderie");
        var updated = await db.Orders.FirstAsync();
        updated.AssignedEmbroidererId.Should().Be(embroidererId);
    }
}
