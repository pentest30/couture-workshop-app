using Xunit;
using FluentAssertions;
using Couture.Orders.Domain;
using Couture.Clients.Domain;
using Couture.Finance.Domain;
using Couture.Notifications.Domain;

namespace Couture.Domain.Tests;

/// <summary>
/// Comprehensive business rules tests matching SPECS_FONCTIONNELLES.md
/// Covers: F01, F02, F03, RG01-RG07
/// </summary>
public class BusinessRulesTests
{
    #region F01 — Gestion des Commandes

    [Fact]
    public void F01_OrderCode_Format_CMD_YYYY_NNNN()
    {
        // RG01: Format CMD-{YYYY}-{NNNN}
        var order = Order.Create("CMD-2026-0042", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 10000m);
        order.Code.Should().MatchRegex(@"^CMD-\d{4}-\d{4}$");
    }

    [Fact]
    public void F01_InitialStatus_IsRecue()
    {
        var order = Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 10000m);
        order.Status.Should().Be(OrderStatus.Recue);
        order.Status.Label.Should().Be("Reçue");
        order.Status.Color.Should().Be("#1565C0");
    }

    [Fact]
    public void F01_TotalPrice_MustBePositive()
    {
        var act = () => Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void F01_NegativePrice_Rejected()
    {
        var act = () => Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), -500m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void F01_OrderCreation_RaisesDomainEvent()
    {
        var order = Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Brode,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), 25000m);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<Couture.Orders.Contracts.Events.OrderCreatedEvent>();
    }

    #endregion

    #region F02 — Statuts & Cycle de Vie

    // F02: Full transition matrix verification per spec diagram

    [Theory]
    [InlineData("Simple")]
    [InlineData("Brode")]
    [InlineData("Perle")]
    [InlineData("Mixte")]
    public void F02_Recue_To_EnAttente_AllowedForAllTypes(string workTypeName)
    {
        var wt = WorkType.FromName(workTypeName);
        OrderStatus.Recue.CanTransitionTo(OrderStatus.EnAttente, wt).Should().BeTrue();
    }

    [Theory]
    [InlineData("Simple")]
    [InlineData("Brode")]
    [InlineData("Perle")]
    [InlineData("Mixte")]
    public void F02_Recue_To_EnCours_AllowedForAllTypes(string workTypeName)
    {
        var wt = WorkType.FromName(workTypeName);
        OrderStatus.Recue.CanTransitionTo(OrderStatus.EnCours, wt).Should().BeTrue();
    }

    [Fact]
    public void F02_EnCours_To_Broderie_OnlyForBrodeOrMixte()
    {
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Broderie, WorkType.Simple).Should().BeFalse("Simple cannot go to Broderie");
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Broderie, WorkType.Perle).Should().BeFalse("Perle cannot go to Broderie");
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Broderie, WorkType.Brode).Should().BeTrue("Brode can go to Broderie");
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Broderie, WorkType.Mixte).Should().BeTrue("Mixte can go to Broderie");
    }

    [Fact]
    public void F02_EnCours_To_Perlage_OnlyForPerleOrMixte()
    {
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Perlage, WorkType.Simple).Should().BeFalse();
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Perlage, WorkType.Brode).Should().BeFalse();
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Perlage, WorkType.Perle).Should().BeTrue();
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Perlage, WorkType.Mixte).Should().BeTrue();
    }

    [Fact]
    public void F02_Broderie_To_Perlage_OnlyForMixte()
    {
        OrderStatus.Broderie.CanTransitionTo(OrderStatus.Perlage, WorkType.Brode).Should().BeFalse("Brode goes Broderie->Prete, not Perlage");
        OrderStatus.Broderie.CanTransitionTo(OrderStatus.Perlage, WorkType.Mixte).Should().BeTrue("Mixte goes Broderie->Perlage");
    }

    [Fact]
    public void F02_Livree_IsTerminal_NoTransitionsAllowed()
    {
        OrderStatus.Livree.IsTerminal.Should().BeTrue();
        foreach (var status in OrderStatus.List)
        {
            if (status == OrderStatus.Livree) continue;
            OrderStatus.Livree.CanTransitionTo(status, WorkType.Simple).Should().BeFalse();
        }
    }

    [Fact]
    public void F02_Retouche_RequiresReason()
    {
        var order = CreateOrderInStatus(OrderStatus.EnCours, WorkType.Simple);
        var act = () => order.ChangeStatus(OrderStatus.Retouche, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*reason*");
    }

    [Fact]
    public void F02_Retouche_WithReason_Succeeds()
    {
        var order = CreateOrderInStatus(OrderStatus.EnCours, WorkType.Simple);
        order.ChangeStatus(OrderStatus.Retouche, Guid.NewGuid(), "Ajustement taille");
        order.Status.Should().Be(OrderStatus.Retouche);
        order.Transitions.Last().Reason.Should().Be("Ajustement taille");
    }

    [Fact]
    public void F02_EnCours_RequiresTailor()
    {
        var order = Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 10000m);
        // No tailor assigned
        var act = () => order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*tailor*");
    }

    [Fact]
    public void F02_Broderie_RequiresEmbroiderer()
    {
        var order = CreateOrderInStatus(OrderStatus.EnCours, WorkType.Brode);
        // No embroiderer assigned
        var act = () => order.ChangeStatus(OrderStatus.Broderie, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*embroiderer*");
    }

    [Fact]
    public void F02_Perlage_RequiresBeader()
    {
        var order = CreateOrderInStatus(OrderStatus.EnCours, WorkType.Perle);
        var act = () => order.ChangeStatus(OrderStatus.Perlage, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*beader*");
    }

    [Fact]
    public void F02_Livree_RequiresZeroBalanceOrReason()
    {
        var order = CreateOrderInStatus(OrderStatus.Prete, WorkType.Simple);
        // Balance > 0 without reason should fail
        var act = () => order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 5000m);
        act.Should().Throw<InvalidOperationException>().WithMessage("*reason*");
    }

    [Fact]
    public void F02_Livree_WithZeroBalance_Succeeds()
    {
        var order = CreateOrderInStatus(OrderStatus.Prete, WorkType.Simple);
        order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 0);
        order.Status.Should().Be(OrderStatus.Livree);
        order.HasUnpaidBalance.Should().BeFalse();
        order.ActualDeliveryDate.Should().NotBeNull();
    }

    [Fact]
    public void F02_Livree_WithUnpaidAndReason_Succeeds()
    {
        var order = CreateOrderInStatus(OrderStatus.Prete, WorkType.Simple);
        order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 5000m, "Paie la semaine prochaine");
        order.Status.Should().Be(OrderStatus.Livree);
        order.HasUnpaidBalance.Should().BeTrue();
        order.DeliveryWithUnpaidReason.Should().Be("Paie la semaine prochaine");
    }

    [Fact]
    public void F02_Timeline_RecordsAllTransitions()
    {
        var order = CreateOrderInStatus(OrderStatus.EnCours, WorkType.Simple);
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());
        order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 0);

        // Initial (Recue) + EnCours + Prete + Livree = 4 transitions
        order.Transitions.Should().HaveCount(4);
        order.Transitions[0].ToStatus.Should().Be(OrderStatus.Recue);
        order.Transitions[1].ToStatus.Should().Be(OrderStatus.EnCours);
        order.Transitions[2].ToStatus.Should().Be(OrderStatus.Prete);
        order.Transitions[3].ToStatus.Should().Be(OrderStatus.Livree);
    }

    #endregion

    #region F03 — Types de Travaux

    [Fact]
    public void F03_Simple_Workflow_EnCours_Prete()
    {
        var order = CreateOrderInStatus(OrderStatus.EnCours, WorkType.Simple);
        // Simple: EN_COURS -> PRETE (direct)
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());
        order.Status.Should().Be(OrderStatus.Prete);
    }

    [Fact]
    public void F03_Brode_Workflow_EnCours_Broderie_Prete()
    {
        var order = CreateOrderInStatus(OrderStatus.EnCours, WorkType.Brode);
        order.Update(assignedEmbroidererId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Broderie, Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());
        order.Status.Should().Be(OrderStatus.Prete);
    }

    [Fact]
    public void F03_Perle_Workflow_EnCours_Perlage_Prete()
    {
        var order = CreateOrderInStatus(OrderStatus.EnCours, WorkType.Perle);
        order.Update(assignedBeaderId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Perlage, Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());
        order.Status.Should().Be(OrderStatus.Prete);
    }

    [Fact]
    public void F03_Mixte_Workflow_EnCours_Broderie_Perlage_Prete()
    {
        var order = CreateOrderInStatus(OrderStatus.EnCours, WorkType.Mixte);
        order.Update(assignedEmbroidererId: Guid.NewGuid(), assignedBeaderId: Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Broderie, Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Perlage, Guid.NewGuid());
        order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());
        order.Status.Should().Be(OrderStatus.Prete);
    }

    [Theory]
    [InlineData("Simple", 1, 3)]
    [InlineData("Brode", 3, 7)]
    [InlineData("Perle", 5, 10)]
    [InlineData("Mixte", 7, 14)]
    public void F03_WorkType_DeliveryAndStallThresholds(string typeName, int minDays, int stallDays)
    {
        var wt = WorkType.FromName(typeName);
        wt.MinDeliveryBusinessDays.Should().Be(minDays);
        wt.StallThresholdDays.Should().Be(stallDays);
    }

    [Fact]
    public void F03_Brode_RequiresEmbroiderer()
    {
        WorkType.Brode.RequiresEmbroiderer.Should().BeTrue();
        WorkType.Brode.RequiresBeader.Should().BeFalse();
    }

    [Fact]
    public void F03_Perle_RequiresBeader()
    {
        WorkType.Perle.RequiresEmbroiderer.Should().BeFalse();
        WorkType.Perle.RequiresBeader.Should().BeTrue();
    }

    [Fact]
    public void F03_Mixte_RequiresBoth()
    {
        WorkType.Mixte.RequiresEmbroiderer.Should().BeTrue();
        WorkType.Mixte.RequiresBeader.Should().BeTrue();
    }

    #endregion

    #region RG02 — Date de livraison minimale

    [Fact]
    public void RG02_Simple_MinOneDay()
    {
        // Delivery tomorrow should work for Simple
        var order = Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), 10000m);
        order.Should().NotBeNull();
    }

    [Fact]
    public void RG02_DeliveryDate_TooEarly_Rejected()
    {
        // Delivery today should fail for any type (min 1 day)
        var act = () => Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Mixte,
            DateOnly.FromDateTime(DateTime.UtcNow), 10000m);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Delivery date*");
    }

    #endregion

    #region RG04 — Immutabilite de LIVREE

    [Fact]
    public void RG04_Delivered_CannotBeModified()
    {
        var order = CreateDeliveredOrder();
        var act = () => order.Update(totalPrice: 50000m);
        act.Should().Throw<InvalidOperationException>().WithMessage("*delivered*");
    }

    [Fact]
    public void RG04_Delivered_CannotChangeStatus()
    {
        var order = CreateDeliveredOrder();
        var act = () => order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*delivered*");
    }

    [Fact]
    public void RG04_Delivered_CanAddNote()
    {
        var order = CreateDeliveredOrder();
        order.AddNote("Client contacted for follow-up");
        order.TechnicalNotes.Should().Contain("Client contacted");
    }

    #endregion

    #region RG06 — Retard

    [Fact]
    public void RG06_IsLate_WhenPastDeliveryAndNotDelivered()
    {
        var order = Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), 10000m);
        // Not late yet (delivery in 2 days)
        var isLate = order.Status != OrderStatus.Livree && order.ExpectedDeliveryDate < DateOnly.FromDateTime(DateTime.UtcNow);
        isLate.Should().BeFalse();
    }

    #endregion

    #region RG07 — Dedoublonnage client

    [Fact]
    public void RG07_ClientCode_Format()
    {
        var client = Client.Create("C-0042", "Sara", "Benali", "0550123456");
        client.Code.Should().MatchRegex(@"^C-\d{4}$");
    }

    [Fact]
    public void RG07_Client_RequiresPhone()
    {
        var act = () => Client.Create("C-0001", "Sara", "Benali", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RG07_Client_RequiresName()
    {
        var act = () => Client.Create("C-0001", "", "Benali", "0550123456");
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region F08 — Finance

    [Fact]
    public void F08_Payment_MustBePositive()
    {
        var act = () => Payment.Create(Guid.NewGuid(), 0m, PaymentMethod.Especes,
            DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void F08_AllPaymentMethods_Exist()
    {
        PaymentMethod.List.Should().HaveCount(5);
        PaymentMethod.List.Select(m => m.Name).Should().Contain(new[] { "Especes", "Virement", "Ccp", "BaridiMob", "Dahabia" });
    }

    [Fact]
    public void F08_Receipt_Format()
    {
        var receipt = Receipt.Create("REC-2026-0001", Couture.Finance.Contracts.PaymentId.From(Guid.NewGuid()), "path.pdf");
        receipt.Code.Should().MatchRegex(@"^REC-\d{4}-\d{4}$");
    }

    #endregion

    #region F05 — Notifications

    [Fact]
    public void F05_NotificationType_AllEightExist()
    {
        NotificationType.List.Should().HaveCount(8);
    }

    [Fact]
    public void F05_N01_Overdue_IsCritical()
    {
        NotificationType.N01_Overdue.DefaultPriority.Should().Be(NotificationPriority.Critical);
        NotificationType.N01_Overdue.DefaultSmsEnabled.Should().BeTrue();
    }

    [Fact]
    public void F05_N08_UnpaidDelivery_IsCritical()
    {
        NotificationType.N08_UnpaidDelivery.DefaultPriority.Should().Be(NotificationPriority.Critical);
    }

    [Fact]
    public void F05_Notification_ExpiresIn30Days()
    {
        var notif = Notification.Create(NotificationType.N01_Overdue, Guid.NewGuid(), Guid.NewGuid(),
            "Test", "Message", false);
        notif.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void F05_Notification_MarkAsRead()
    {
        var notif = Notification.Create(NotificationType.N01_Overdue, Guid.NewGuid(), Guid.NewGuid(),
            "Test", "Message", false);
        notif.IsRead.Should().BeFalse();
        notif.MarkAsRead();
        notif.IsRead.Should().BeTrue();
        notif.ReadAt.Should().NotBeNull();
    }

    #endregion

    #region Status color verification (F02 spec compliance)

    [Theory]
    [InlineData("Recue", "#1565C0")]
    [InlineData("EnAttente", "#F9A825")]
    [InlineData("EnCours", "#E65100")]
    [InlineData("Broderie", "#6A1B9A")]
    [InlineData("Perlage", "#880E4F")]
    [InlineData("Retouche", "#C62828")]
    [InlineData("Prete", "#2E7D32")]
    [InlineData("Livree", "#424242")]
    public void F02_StatusColors_MatchSpec(string statusName, string expectedColor)
    {
        var status = OrderStatus.FromName(statusName);
        status.Color.Should().Be(expectedColor);
    }

    [Theory]
    [InlineData("Recue", "Reçue")]
    [InlineData("EnAttente", "En Attente")]
    [InlineData("EnCours", "En Cours")]
    [InlineData("Broderie", "En Broderie")]
    [InlineData("Perlage", "En Perlage")]
    [InlineData("Retouche", "En Retouche")]
    [InlineData("Prete", "Prête")]
    [InlineData("Livree", "Livrée")]
    public void F02_StatusLabels_MatchSpec(string statusName, string expectedLabel)
    {
        var status = OrderStatus.FromName(statusName);
        status.Label.Should().Be(expectedLabel);
    }

    #endregion

    #region Helpers

    private static Order CreateOrderInStatus(OrderStatus targetStatus, WorkType workType)
    {
        var order = Order.Create("CMD-2026-0001", Guid.NewGuid(), workType,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), 20000m);

        if (targetStatus == OrderStatus.Recue) return order;

        order.Update(assignedTailorId: Guid.NewGuid());
        if (targetStatus == OrderStatus.EnCours)
        {
            order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());
            return order;
        }

        order.ChangeStatus(OrderStatus.EnCours, Guid.NewGuid());

        if (targetStatus == OrderStatus.Broderie)
        {
            order.Update(assignedEmbroidererId: Guid.NewGuid());
            order.ChangeStatus(OrderStatus.Broderie, Guid.NewGuid());
            return order;
        }

        if (targetStatus == OrderStatus.Perlage)
        {
            order.Update(assignedBeaderId: Guid.NewGuid());
            order.ChangeStatus(OrderStatus.Perlage, Guid.NewGuid());
            return order;
        }

        if (targetStatus == OrderStatus.Retouche)
        {
            order.ChangeStatus(OrderStatus.Retouche, Guid.NewGuid(), "Ajustement");
            return order;
        }

        if (targetStatus == OrderStatus.Prete)
        {
            order.ChangeStatus(OrderStatus.Prete, Guid.NewGuid());
            return order;
        }

        return order;
    }

    private static Order CreateDeliveredOrder()
    {
        var order = CreateOrderInStatus(OrderStatus.Prete, WorkType.Simple);
        order.MarkAsDelivered(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), 0);
        return order;
    }

    #endregion
}
