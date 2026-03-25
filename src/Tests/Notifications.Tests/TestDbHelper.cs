using Couture.Notifications.Persistence;
using Couture.Orders.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Couture.Notifications.Tests;

public static class TestDbHelper
{
    public static (NotificationsDbContext Notif, OrdersDbContext Orders) Create(string? name = null)
    {
        var n = name ?? Guid.NewGuid().ToString();
        return (
            new NotificationsDbContext(new DbContextOptionsBuilder<NotificationsDbContext>().UseInMemoryDatabase(n + "_notif").Options),
            new OrdersDbContext(new DbContextOptionsBuilder<OrdersDbContext>().UseInMemoryDatabase(n + "_orders").Options));
    }
}
