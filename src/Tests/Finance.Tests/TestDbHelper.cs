using Couture.Finance.Persistence;
using Couture.Orders.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Couture.Finance.Tests;

public static class TestDbHelper
{
    public static (FinanceDbContext Finance, OrdersDbContext Orders) CreateInMemoryContexts(string? dbName = null)
    {
        var name = dbName ?? Guid.NewGuid().ToString();

        var financeOptions = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(name + "_finance")
            .Options;

        var ordersOptions = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(name + "_orders")
            .Options;

        return (new FinanceDbContext(financeOptions), new OrdersDbContext(ordersOptions));
    }
}
