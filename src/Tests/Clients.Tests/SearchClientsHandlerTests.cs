using Xunit;
using Couture.Clients.Domain;
using Couture.Clients.Features.SearchClients;
using FluentAssertions;

namespace Couture.Clients.Tests;

public class SearchClientsHandlerTests
{
    private static void SeedClients(Couture.Clients.Persistence.ClientsDbContext db)
    {
        db.Clients.AddRange(
            Client.Create("C-0001", "Sara", "Benali", "0550111111"),
            Client.Create("C-0002", "Nadia", "Hamidi", "0661222222"),
            Client.Create("C-0003", "Fatima", "Benali", "0770333333"));
        db.SaveChanges();
    }

    [Fact]
    public async Task Handle_SearchByLastName_ReturnsMatches()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        SeedClients(db);
        var handler = new SearchClientsHandler(db);

        var results = await handler.Handle(new SearchClientsQuery("benali"), CancellationToken.None);

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(c => c.LastName.Should().Be("Benali"));
    }

    [Fact]
    public async Task Handle_SearchByCode_ReturnsMatch()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        SeedClients(db);
        var handler = new SearchClientsHandler(db);

        var results = await handler.Handle(new SearchClientsQuery("C-0002"), CancellationToken.None);

        results.Should().ContainSingle().Which.FirstName.Should().Be("Nadia");
    }

    [Fact]
    public async Task Handle_SearchByPhone_ReturnsMatch()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        SeedClients(db);
        var handler = new SearchClientsHandler(db);

        var results = await handler.Handle(new SearchClientsQuery("0661"), CancellationToken.None);

        results.Should().ContainSingle().Which.FirstName.Should().Be("Nadia");
    }

    [Fact]
    public async Task Handle_ShortQuery_ReturnsEmpty()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        SeedClients(db);
        var handler = new SearchClientsHandler(db);

        var results = await handler.Handle(new SearchClientsQuery("a"), CancellationToken.None);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MaxTenResults()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        for (int i = 1; i <= 15; i++)
            db.Clients.Add(Client.Create($"C-{i:D4}", "Test", $"Client{i}", $"055000{i:D4}"));
        db.SaveChanges();
        var handler = new SearchClientsHandler(db);

        var results = await handler.Handle(new SearchClientsQuery("test"), CancellationToken.None);

        results.Should().HaveCount(10);
    }
}
