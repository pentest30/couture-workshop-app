using Couture.Clients.Persistence;
using Couture.Finance.Persistence;
using Couture.Notifications.Persistence;
using Couture.Orders.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Couture.Integration.Tests;

public sealed class TestDatabases
{
    public OrdersDbContext Orders { get; }
    public ClientsDbContext Clients { get; }
    public FinanceDbContext Finance { get; }
    public NotificationsDbContext Notifications { get; }

    public TestDatabases(string? name = null)
    {
        var n = name ?? Guid.NewGuid().ToString();
        Orders = new(new DbContextOptionsBuilder<OrdersDbContext>().UseInMemoryDatabase(n + "_orders").Options);
        Clients = new(new DbContextOptionsBuilder<ClientsDbContext>().UseInMemoryDatabase(n + "_clients").Options);
        Finance = new(new DbContextOptionsBuilder<FinanceDbContext>().UseInMemoryDatabase(n + "_finance").Options);
        Notifications = new(new DbContextOptionsBuilder<NotificationsDbContext>().UseInMemoryDatabase(n + "_notif").Options);
    }
}
