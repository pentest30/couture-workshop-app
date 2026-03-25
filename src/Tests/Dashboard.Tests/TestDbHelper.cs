using Couture.Finance.Persistence;
using Couture.Orders.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Couture.Dashboard.Tests;

public static class TestDbHelper
{
    public static (OrdersDbContext Orders, FinanceDbContext Finance) Create(string? name = null)
    {
        var n = name ?? Guid.NewGuid().ToString();
        return (
            new OrdersDbContext(new DbContextOptionsBuilder<OrdersDbContext>().UseInMemoryDatabase(n + "_orders").Options),
            new FinanceDbContext(new DbContextOptionsBuilder<FinanceDbContext>().UseInMemoryDatabase(n + "_finance").Options));
    }
}
