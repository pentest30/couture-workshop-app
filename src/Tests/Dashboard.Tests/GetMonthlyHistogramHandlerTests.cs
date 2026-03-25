using Xunit;
using FluentAssertions;
using Couture.Dashboard.Features.GetMonthlyHistogram;
using Couture.Orders.Domain;

namespace Couture.Dashboard.Tests;

public class GetMonthlyHistogramHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsThreeMonths_WithCurrentMonthData()
    {
        var (ordersDb, _) = TestDbHelper.Create();

        // Order.Create sets ReceptionDate = today (March 2026)
        // All orders will be in the current month
        ordersDb.Orders.AddRange(
            Order.Create("CMD-2026-0001", Guid.NewGuid(), WorkType.Simple, new DateOnly(2026, 4, 15), 10000m),
            Order.Create("CMD-2026-0002", Guid.NewGuid(), WorkType.Brode, new DateOnly(2026, 4, 20), 20000m),
            Order.Create("CMD-2026-0003", Guid.NewGuid(), WorkType.Perle, new DateOnly(2026, 4, 10), 15000m));
        await ordersDb.SaveChangesAsync();

        var handler = new GetMonthlyHistogramHandler(ordersDb);
        var result = await handler.Handle(new GetMonthlyHistogramQuery(2026, 1), CancellationToken.None);

        result.Months.Should().HaveCount(3); // Jan, Feb, Mar

        // All orders have ReceptionDate = today (March), so they're all in Month[2]
        var marchData = result.Months[2];
        marchData.Simple.Should().Be(1);
        marchData.Embroidered.Should().Be(1);
        marchData.Beaded.Should().Be(1);

        // Jan and Feb should be empty
        result.Months[0].Simple.Should().Be(0);
        result.Months[1].Simple.Should().Be(0);
    }
}
