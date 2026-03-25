using Xunit;
using Couture.Orders.Domain;
using FluentAssertions;

namespace Couture.Domain.Tests;

public class WorkTypeTests
{
    [Fact]
    public void Simple_DoesNotRequireEmbroidererOrBeader()
    {
        WorkType.Simple.RequiresEmbroiderer.Should().BeFalse();
        WorkType.Simple.RequiresBeader.Should().BeFalse();
    }

    [Fact]
    public void Brode_RequiresEmbroiderer_NotBeader()
    {
        WorkType.Brode.RequiresEmbroiderer.Should().BeTrue();
        WorkType.Brode.RequiresBeader.Should().BeFalse();
    }

    [Fact]
    public void Perle_RequiresBeader_NotEmbroiderer()
    {
        WorkType.Perle.RequiresEmbroiderer.Should().BeFalse();
        WorkType.Perle.RequiresBeader.Should().BeTrue();
    }

    [Fact]
    public void Mixte_RequiresBoth()
    {
        WorkType.Mixte.RequiresEmbroiderer.Should().BeTrue();
        WorkType.Mixte.RequiresBeader.Should().BeTrue();
    }

    [Theory]
    [InlineData("Simple", 1)]
    [InlineData("Brode", 3)]
    [InlineData("Perle", 5)]
    [InlineData("Mixte", 7)]
    public void MinDeliveryBusinessDays_MatchesSpec(string typeName, int expectedDays)
    {
        var wt = WorkType.FromName(typeName);
        wt.MinDeliveryBusinessDays.Should().Be(expectedDays);
    }

    [Theory]
    [InlineData("Simple", 3)]
    [InlineData("Brode", 7)]
    [InlineData("Perle", 10)]
    [InlineData("Mixte", 14)]
    public void StallThresholdDays_MatchesSpec(string typeName, int expectedDays)
    {
        var wt = WorkType.FromName(typeName);
        wt.StallThresholdDays.Should().Be(expectedDays);
    }
}
