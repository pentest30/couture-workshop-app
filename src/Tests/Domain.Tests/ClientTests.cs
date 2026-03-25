using Xunit;
using Couture.Clients.Domain;
using FluentAssertions;

namespace Couture.Domain.Tests;

public class ClientTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var client = Client.Create("C-0001", "Sara", "Benali", "0550123456");
        client.FirstName.Should().Be("Sara");
        client.LastName.Should().Be("Benali");
        client.FullName.Should().Be("Sara Benali");
        client.Code.Should().Be("C-0001");
    }

    [Fact]
    public void Create_WithEmptyFirstName_Throws()
    {
        var act = () => Client.Create("C-0001", "", "Benali", "0550123456");
        act.Should().Throw<ArgumentException>().WithMessage("*First name*");
    }

    [Fact]
    public void Create_WithEmptyPhone_Throws()
    {
        var act = () => Client.Create("C-0001", "Sara", "Benali", "");
        act.Should().Throw<ArgumentException>().WithMessage("*phone*");
    }

    [Fact]
    public void Update_ChangesFields()
    {
        var client = Client.Create("C-0001", "Sara", "Benali", "0550123456");
        client.Update(firstName: "Nadia", lastName: "Hamidi");
        client.FirstName.Should().Be("Nadia");
        client.LastName.Should().Be("Hamidi");
    }
}
