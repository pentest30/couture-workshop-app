using Xunit;
using Couture.Finance.Domain;
using FluentAssertions;

namespace Couture.Domain.Tests;

public class PaymentTests
{
    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var payment = Payment.Create(Guid.NewGuid(), 5000m, PaymentMethod.Especes,
            DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid(), "Acompte");

        payment.Amount.Should().Be(5000m);
        payment.PaymentMethod.Should().Be(PaymentMethod.Especes);
        payment.Note.Should().Be("Acompte");
    }

    [Fact]
    public void Create_WithZeroAmount_Throws()
    {
        var act = () => Payment.Create(Guid.NewGuid(), 0m, PaymentMethod.Especes,
            DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    [Fact]
    public void Create_WithNegativeAmount_Throws()
    {
        var act = () => Payment.Create(Guid.NewGuid(), -100m, PaymentMethod.Especes,
            DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    [Fact]
    public void PaymentMethod_AllFiveExist()
    {
        PaymentMethod.List.Should().HaveCount(5);
        PaymentMethod.Especes.Label.Should().Be("Espèces");
        PaymentMethod.BaridiMob.Label.Should().Be("BaridiMob");
    }
}
