using Xunit;
using Couture.Orders.Domain;
using FluentAssertions;

namespace Couture.Domain.Tests;

public class OrderAggregateTests
{
    private Order CreateSimpleOrder() => Order.Create(
        "CMD-2026-0001", Guid.NewGuid(), WorkType.Simple,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 10000m,
        description: "Test order");

    private Order CreateBrodeOrder() => Order.Create(
        "CMD-2026-0002", Guid.NewGuid(), WorkType.Brode,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), 25000m);

    [Fact]
    public void Create_SetsInitialStatus_Recue()
    {
        var order = CreateSimpleOrder();
        order.Status.Should().Be(OrderStatus.Recue);
    }

    [Fact]
    public void Create_GeneratesOrderCreatedEvent()
    {
        var order = CreateSimpleOrder();
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<Couture.Orders.Contracts.Events.OrderCreatedEvent>();
    }

    [Fact]
    public void Create_SetsInitialTransition()
    {
        var order = CreateSimpleOrder();
        order.Transitions.Should().ContainSingle()
            .Which.ToStatus.Should().Be(OrderStatus.Recue);
    }

    [Fact]
    public void Create_WithZeroPrice_Throws()
    {
        var act = () => Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0m);
        act.Should().Throw<InvalidOperationException>().WithMessage("*price*");
    }

    [Fact]
    public void ChangeStatus_ToEnCours_RequiresTailor()
    {
        var order = CreateSimpleOrder();
        // No tailor assigned
        var act = () => order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*tailor*");
    }

    [Fact]
    public void ChangeStatus_ToEnCours_WithTailor_Succeeds()
    {
        var order = CreateSimpleOrder();
        order.Update(assignedTailorId: Guid.NewGuid());
        order.ClearDomainEvents();

        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());

        order.Status.Should().Be(OrderStatus.EnCours);
        order.Transitions.Should().HaveCount(2); // initial + this one
    }

    [Fact]
    public void ChangeStatus_ToBroderie_RequiresEmbroiderer()
    {
        var order = CreateBrodeOrder();
        order.Update(assignedTailorId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());

        var act = () => order.ChangeStatus(OrderStatus.Broderie, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*embroiderer*");
    }

    [Fact]
    public void ChangeStatus_ToBroderie_WithEmbroiderer_Succeeds()
    {
        var order = CreateBrodeOrder();
        order.Update(assignedTailorId: Guid.NewGuid(), assignedEmbroidererId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        order.ClearDomainEvents();

        order.ChangeStatus(OrderStatus.Broderie, Guid.NewGuid());

        order.Status.Should().Be(OrderStatus.Broderie);
    }

    [Fact]
    public void ChangeStatus_ToRetouche_RequiresReason()
    {
        var order = CreateSimpleOrder();
        order.Update(assignedTailorId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());

        var act = () => order.ChangeStatus(OrderStatus.Retouche, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*reason*");
    }

    [Fact]
    public void ChangeStatus_ToRetouche_WithReason_Succeeds()
    {
        var order = CreateSimpleOrder();
        order.Update(assignedTailorId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        order.ClearDomainEvents();

        order.ChangeStatus(OrderStatus.Retouche, Guid.NewGuid(), "Ajustement taille");

        order.Status.Should().Be(OrderStatus.Retouche);
        order.Transitions.Last().Reason.Should().Be("Ajustement taille");
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_Throws()
    {
        var order = CreateSimpleOrder();
        // Recue -> Prete is not valid
        var act = () => order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*Cannot transition*");
    }

    [Fact]
    public void ChangeStatus_AfterDelivered_Throws()
    {
        var order = CreateSimpleOrder();
        order.Update(assignedTailorId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());
        order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 0);

        var act = () => order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*delivered*");
    }

    [Fact]
    public void MarkAsDelivered_WithUnpaidBalance_RequiresReason()
    {
        var order = CreateSimpleOrder();
        order.Update(assignedTailorId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());

        var act = () => order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 5000m);
        act.Should().Throw<InvalidOperationException>().WithMessage("*reason*");
    }

    [Fact]
    public void MarkAsDelivered_WithUnpaidBalance_AndReason_Succeeds()
    {
        var order = CreateSimpleOrder();
        order.Update(assignedTailorId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());

        order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 5000m, "Client paie la semaine prochaine");

        order.Status.Should().Be(OrderStatus.Livree);
        order.HasUnpaidBalance.Should().BeTrue();
        order.DeliveryWithUnpaidReason.Should().Be("Client paie la semaine prochaine");
    }

    [Fact]
    public void Update_AfterDelivered_Throws()
    {
        var order = CreateSimpleOrder();
        order.Update(assignedTailorId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());
        order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 0);

        var act = () => order.Update(totalPrice: 20000m);
        act.Should().Throw<InvalidOperationException>().WithMessage("*delivered*");
    }
}
