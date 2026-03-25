using Xunit;
using Couture.Orders.Domain;
using FluentAssertions;

namespace Couture.Domain.Tests;

public class OrderStatusTransitionTests
{
    [Fact]
    public void Recue_CanTransitionTo_EnAttente()
    {
        OrderStatus.Recue.CanTransitionTo(OrderStatus.EnAttente, WorkType.Simple)
            .Should().BeTrue();
    }

    [Fact]
    public void Recue_CanTransitionTo_EnCours()
    {
        OrderStatus.Recue.CanTransitionTo(OrderStatus.EnCours, WorkType.Simple)
            .Should().BeTrue();
    }

    [Fact]
    public void EnCours_CanTransitionTo_Broderie_ForBrodeType()
    {
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Broderie, WorkType.Brode)
            .Should().BeTrue();
    }

    [Fact]
    public void EnCours_CannotTransitionTo_Broderie_ForSimpleType()
    {
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Broderie, WorkType.Simple)
            .Should().BeFalse();
    }

    [Fact]
    public void EnCours_CanTransitionTo_Perlage_ForPerleType()
    {
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Perlage, WorkType.Perle)
            .Should().BeTrue();
    }

    [Fact]
    public void EnCours_CannotTransitionTo_Perlage_ForBrodeType()
    {
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Perlage, WorkType.Brode)
            .Should().BeFalse();
    }

    [Fact]
    public void Broderie_CanTransitionTo_Perlage_ForMixteOnly()
    {
        OrderStatus.Broderie.CanTransitionTo(OrderStatus.Perlage, WorkType.Mixte)
            .Should().BeTrue();

        OrderStatus.Broderie.CanTransitionTo(OrderStatus.Perlage, WorkType.Brode)
            .Should().BeFalse();
    }

    [Fact]
    public void Prete_CanTransitionTo_Livree()
    {
        OrderStatus.Prete.CanTransitionTo(OrderStatus.Livree, WorkType.Simple)
            .Should().BeTrue();
    }

    [Fact]
    public void Livree_CannotTransitionToAnything()
    {
        foreach (var status in OrderStatus.List)
        {
            if (status == OrderStatus.Livree) continue;
            OrderStatus.Livree.CanTransitionTo(status, WorkType.Simple)
                .Should().BeFalse($"Livree should not transition to {status.Name}");
        }
    }

    [Fact]
    public void CannotTransitionToSelf()
    {
        foreach (var status in OrderStatus.List)
        {
            status.CanTransitionTo(status, WorkType.Simple)
                .Should().BeFalse($"{status.Name} should not transition to itself");
        }
    }

    [Fact]
    public void Retouche_CanTransitionTo_EnCours()
    {
        OrderStatus.Retouche.CanTransitionTo(OrderStatus.EnCours, WorkType.Simple)
            .Should().BeTrue();
    }

    [Fact]
    public void Retouche_CanTransitionTo_Prete()
    {
        OrderStatus.Retouche.CanTransitionTo(OrderStatus.Prete, WorkType.Simple)
            .Should().BeTrue();
    }

    [Fact]
    public void EnCours_CanTransitionTo_Retouche()
    {
        OrderStatus.EnCours.CanTransitionTo(OrderStatus.Retouche, WorkType.Simple)
            .Should().BeTrue();
    }
}
