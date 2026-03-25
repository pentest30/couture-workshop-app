using Xunit;
using Couture.Orders.Domain;
using Couture.Orders.Features.CreateOrder;
using FluentAssertions;

namespace Couture.Orders.Tests;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesOrder()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateOrderHandler(db);

        var command = new CreateOrderCommand(
            ClientId: Guid.NewGuid(),
            WorkType: "Simple",
            ExpectedDeliveryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            TotalPrice: 12000m,
            InitialDeposit: 3000m,
            DepositPaymentMethod: "Especes",
            Description: "Robe simple",
            Fabric: "Satin",
            TechnicalNotes: null,
            AssignedTailorId: null,
            AssignedEmbroidererId: null,
            AssignedBeaderId: null,
            EmbroideryStyle: null, ThreadColors: null, Density: null, EmbroideryZone: null,
            BeadType: null, Arrangement: null, AffectedZones: null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().StartWith("CMD-");
        result.Status.Should().Be("Recue");
        result.OutstandingBalance.Should().Be(9000m);

        var order = db.Orders.First();
        order.WorkType.Should().Be(WorkType.Simple);
        order.TotalPrice.Should().Be(12000m);
    }

    [Fact]
    public async Task Handle_BrodeType_CreatesOrder()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateOrderHandler(db);

        var command = new CreateOrderCommand(
            Guid.NewGuid(), "Brode",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), 25000m,
            5000m, "Virement", "Caftan brodé", "Velours", null,
            null, null, null,
            "Florale", "Or et bordeaux", "Dense", "Corsage",
            null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.OutstandingBalance.Should().Be(20000m);
        db.Orders.First().WorkType.Should().Be(WorkType.Brode);
    }

    [Fact]
    public async Task Handle_SequentialCodes_AreUnique()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateOrderHandler(db);

        var cmd = new CreateOrderCommand(
            Guid.NewGuid(), "Simple",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 10000m,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null);

        var r1 = await handler.Handle(cmd, CancellationToken.None);
        var r2 = await handler.Handle(cmd with { ClientId = Guid.NewGuid() }, CancellationToken.None);

        r1.Code.Should().NotBe(r2.Code);
        r1.Code.Should().EndWith("0001");
        r2.Code.Should().EndWith("0002");
    }
}
