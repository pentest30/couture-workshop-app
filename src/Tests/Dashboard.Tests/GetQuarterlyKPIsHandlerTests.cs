using Xunit;
using FluentAssertions;
using Couture.Dashboard.Features.GetQuarterlyKPIs;
using Couture.Orders.Domain;
using Couture.Finance.Domain;

namespace Couture.Dashboard.Tests;

public class GetQuarterlyKPIsHandlerTests
{
    [Fact]
    public async Task Handle_WithOrders_ReturnsCorrectKPIs()
    {
        var (ordersDb, financeDb) = TestDbHelper.Create();

        // Seed 3 orders in Q1 2026 (Jan-Mar)
        ordersDb.Orders.AddRange(
            Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple, new DateOnly(2026, 3, 15), 10000m),
            Order.Create("CMD-2026-0002", Guid.NewGuid(), WorkType.Brode, new DateOnly(2026, 3, 20), 20000m),
            Order.Create("CMD-2026-0003", Guid.NewGuid(), WorkType.Mixte, new DateOnly(2026, 4, 15), 30000m));
        await ordersDb.SaveChangesAsync();

        // Seed payment
        financeDb.Payments.Add(Payment.Create(ordersDb.Orders.First().Id.Value, 5000m, PaymentMethod.Especes,
            new DateOnly(2026, 2, 1), Guid.NewGuid()));
        await financeDb.SaveChangesAsync();

        var handler = new GetQuarterlyKPIsHandler(ordersDb, financeDb);
        var result = await handler.Handle(new GetQuarterlyKPIsQuery(2026, 1), CancellationToken.None);

        result.TotalOrders.Should().Be(3);
        result.RevenueCollected.Should().Be(5000m);
        result.EmbroideredOrders.Should().Be(2); // Brode + Mixte
        result.BeadedOrders.Should().Be(1); // Mixte
        result.Quarter.Should().Be("T1 2026");
    }

    [Fact]
    public async Task Handle_EmptyQuarter_ReturnsZeros()
    {
        var (ordersDb, financeDb) = TestDbHelper.Create();
        var handler = new GetQuarterlyKPIsHandler(ordersDb, financeDb);

        var result = await handler.Handle(new GetQuarterlyKPIsQuery(2026, 2), CancellationToken.None);

        result.TotalOrders.Should().Be(0);
        result.RevenueCollected.Should().Be(0);
        result.OnTimeDeliveryRate.Should().Be(100);
        
    }
}
