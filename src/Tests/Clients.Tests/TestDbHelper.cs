using Couture.Clients.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Couture.Clients.Tests;

public static class TestDbHelper
{
    public static ClientsDbContext CreateInMemoryContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ClientsDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new ClientsDbContext(options);
    }
}
