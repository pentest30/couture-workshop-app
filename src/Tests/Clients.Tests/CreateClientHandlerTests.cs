using Xunit;
using Couture.Clients.Features.CreateClient;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Couture.Clients.Tests;

public class CreateClientHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesClient()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateClientHandler(db);

        var result = await handler.Handle(
            new CreateClientCommand("Sara", "Benali", "0550123456", null, null, null, null),
            CancellationToken.None);

        result.Code.Should().StartWith("C-");
        var client = await db.Clients.FirstAsync();
        client.FirstName.Should().Be("Sara");
        client.LastName.Should().Be("Benali");
        client.PrimaryPhone.Should().Be("0550123456");
    }

    [Fact]
    public async Task Handle_DuplicatePhone_Throws()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateClientHandler(db);

        await handler.Handle(
            new CreateClientCommand("Sara", "Benali", "0550123456", null, null, null, null),
            CancellationToken.None);

        var act = async () => await handler.Handle(
            new CreateClientCommand("Nadia", "Hamidi", "0550123456", null, null, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*existe déjà*");
    }

    [Fact]
    public async Task Handle_SequentialCodes()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateClientHandler(db);

        var r1 = await handler.Handle(
            new CreateClientCommand("Sara", "Benali", "0550111111", null, null, null, null),
            CancellationToken.None);
        var r2 = await handler.Handle(
            new CreateClientCommand("Nadia", "Hamidi", "0550222222", null, null, null, null),
            CancellationToken.None);

        r1.Code.Should().Be("C-0001");
        r2.Code.Should().Be("C-0002");
    }

    [Fact]
    public async Task Handle_TrimsWhitespace()
    {
        using var db = TestDbHelper.CreateInMemoryContext();
        var handler = new CreateClientHandler(db);

        await handler.Handle(
            new CreateClientCommand("  Sara  ", "  Benali  ", " 0550123456 ", null, null, null, null),
            CancellationToken.None);

        var client = await db.Clients.FirstAsync();
        client.FirstName.Should().Be("Sara");
        client.PrimaryPhone.Should().Be("0550123456");
    }
}
