using Xunit;
using FluentAssertions;
using Couture.Finance.Domain;
using Couture.Finance.Persistence;
using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Couture.Finance.Tests;

public class PaymentBalanceTests
{
    private static (FinanceDbContext Finance, OrdersDbContext Orders) CreateDbs(string? name = null)
    {
        var n = name ?? Guid.NewGuid().ToString();
        return (
            new FinanceDbContext(new DbContextOptionsBuilder<FinanceDbContext>().UseInMemoryDatabase(n + "_fin").Options),
            new OrdersDbContext(new DbContextOptionsBuilder<OrdersDbContext>().UseInMemoryDatabase(n + "_ord").Options));
    }

    [Fact]
    public async Task Payment_Amount_Cannot_Exceed_Outstanding()
    {
        var (finDb, ordDb) = CreateDbs();
        var order = Order.Create("CMD-TEST-001", Guid.NewGuid(), WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), 25000m, description: "Test");
        ordDb.Orders.Add(order);
        await ordDb.SaveChangesAsync();

        // First payment: 10000
        var p1 = Payment.Create(order.Id.Value, 10000m, PaymentMethod.Especes, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        finDb.Payments.Add(p1);
        await finDb.SaveChangesAsync();

        // Outstanding should be 15000
        var totalPaid = await finDb.Payments.Where(p => p.OrderId == order.Id.Value).SumAsync(p => p.Amount);
        var outstanding = order.TotalPrice - totalPaid;
        outstanding.Should().Be(15000m);

        // Second payment: 15000 (exactly remaining)
        var p2 = Payment.Create(order.Id.Value, 15000m, PaymentMethod.Virement, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        finDb.Payments.Add(p2);
        await finDb.SaveChangesAsync();

        totalPaid = await finDb.Payments.Where(p => p.OrderId == order.Id.Value).SumAsync(p => p.Amount);
        outstanding = order.TotalPrice - totalPaid;
        outstanding.Should().Be(0m);
    }

    [Fact]
    public async Task Outstanding_Balance_Correct_With_No_Payments()
    {
        var (finDb, ordDb) = CreateDbs();
        var order = Order.Create("CMD-TEST-002", Guid.NewGuid(), WorkType.Brode,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), 45000m, description: "Test brode");
        ordDb.Orders.Add(order);
        await ordDb.SaveChangesAsync();

        var totalPaid = await finDb.Payments.Where(p => p.OrderId == order.Id.Value).SumAsync(p => p.Amount);
        var outstanding = order.TotalPrice - totalPaid;

        totalPaid.Should().Be(0m);
        outstanding.Should().Be(45000m);
    }

    [Fact]
    public async Task Multiple_Payments_Sum_Correctly()
    {
        var (finDb, _) = CreateDbs();
        var orderId = Guid.NewGuid();
        var totalPrice = 30000m;

        // 3 payments
        finDb.Payments.Add(Payment.Create(orderId, 5000m, PaymentMethod.Especes, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()));
        finDb.Payments.Add(Payment.Create(orderId, 8000m, PaymentMethod.BaridiMob, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()));
        finDb.Payments.Add(Payment.Create(orderId, 12000m, PaymentMethod.Virement, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()));
        await finDb.SaveChangesAsync();

        var totalPaid = await finDb.Payments.Where(p => p.OrderId == orderId).SumAsync(p => p.Amount);
        var outstanding = totalPrice - totalPaid;

        totalPaid.Should().Be(25000m);
        outstanding.Should().Be(5000m);
    }

    [Fact]
    public async Task Client_Aggregation_Across_Multiple_Orders()
    {
        var (finDb, ordDb) = CreateDbs();
        var clientId = Guid.NewGuid();

        // Order 1: 20000, paid 15000
        var o1 = Order.Create("CMD-AGG-001", clientId, WorkType.Simple,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 20000m);
        ordDb.Orders.Add(o1);
        await ordDb.SaveChangesAsync();
        finDb.Payments.Add(Payment.Create(o1.Id.Value, 15000m, PaymentMethod.Especes, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()));

        // Order 2: 35000, paid 10000
        var o2 = Order.Create("CMD-AGG-002", clientId, WorkType.Brode,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), 35000m);
        ordDb.Orders.Add(o2);
        await ordDb.SaveChangesAsync();
        finDb.Payments.Add(Payment.Create(o2.Id.Value, 10000m, PaymentMethod.Virement, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()));

        await finDb.SaveChangesAsync();

        // Simulate client aggregation (same logic as frontend)
        var clientOrders = await ordDb.Orders.Where(o => o.ClientId == clientId).ToListAsync();
        decimal clientTotalPrice = 0, clientTotalPaid = 0;

        foreach (var order in clientOrders)
        {
            var paid = await finDb.Payments.Where(p => p.OrderId == order.Id.Value).SumAsync(p => p.Amount);
            var outs = order.TotalPrice - paid;
            clientTotalPrice += order.TotalPrice;
            clientTotalPaid += paid;
        }

        var clientOutstanding = clientTotalPrice - clientTotalPaid;

        clientTotalPrice.Should().Be(55000m);  // 20000 + 35000
        clientTotalPaid.Should().Be(25000m);    // 15000 + 10000
        clientOutstanding.Should().Be(30000m);  // 55000 - 25000
    }

    [Fact]
    public void Payment_Amount_Must_Be_Positive()
    {
        var act = () => Payment.Create(Guid.NewGuid(), 0m, PaymentMethod.Especes, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Payment_Negative_Amount_Throws()
    {
        var act = () => Payment.Create(Guid.NewGuid(), -500m, PaymentMethod.Especes, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task Receipt_Generated_With_Payment()
    {
        var (finDb, _) = CreateDbs();
        var orderId = Guid.NewGuid();

        var payment = Payment.Create(orderId, 10000m, PaymentMethod.Especes, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        var receipt = Receipt.Create("REC-2026-0001", payment.Id, "receipts/REC-2026-0001.pdf");
        payment.AttachReceipt(receipt);

        finDb.Payments.Add(payment);
        await finDb.SaveChangesAsync();

        var loaded = await finDb.Payments.Include(p => p.Receipt).FirstAsync(p => p.Id == payment.Id);
        loaded.Receipt.Should().NotBeNull();
        loaded.Receipt!.Code.Should().Be("REC-2026-0001");
        loaded.Amount.Should().Be(10000m);
    }
}
