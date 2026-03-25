using Couture.Orders.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Couture.Orders.Tests;

public static class TestDbHelper
{
    public static OrdersDbContext CreateInMemoryContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new OrdersDbContext(options);
    }
}
