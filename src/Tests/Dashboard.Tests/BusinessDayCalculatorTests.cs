using Xunit;
using FluentAssertions;
using Couture.Dashboard.Domain;
using Couture.Dashboard.Services;

namespace Couture.Dashboard.Tests;

public class BusinessDayCalculatorTests
{
    private static BusinessDayCalculator Create(params Holiday[] holidays) => new(holidays);

    [Fact]
    public void Friday_IsNotBusinessDay()
    {
        var calc = Create();
        calc.IsBusinessDay(new DateOnly(2026, 3, 27)).Should().BeFalse(); // Friday
    }

    [Fact]
    public void Saturday_IsNotBusinessDay()
    {
        var calc = Create();
        calc.IsBusinessDay(new DateOnly(2026, 3, 28)).Should().BeFalse(); // Saturday
    }

    [Fact]
    public void Sunday_IsBusinessDay()
    {
        var calc = Create();
        calc.IsBusinessDay(new DateOnly(2026, 3, 29)).Should().BeTrue(); // Sunday
    }

    [Fact]
    public void Monday_IsBusinessDay()
    {
        var calc = Create();
        calc.IsBusinessDay(new DateOnly(2026, 3, 23)).Should().BeTrue(); // Monday
    }

    [Fact]
    public void Holiday_IsNotBusinessDay()
    {
        var holiday = Holiday.Create(new DateOnly(2026, 7, 5), "Fête de l'Indépendance", true);
        var calc = Create(holiday);
        calc.IsBusinessDay(new DateOnly(2026, 7, 5)).Should().BeFalse();
    }

    [Fact]
    public void CountBusinessDays_ExcludesFridaySaturday()
    {
        var calc = Create();
        // Mon 23 to Thu 26 = 3 business days (Tue, Wed, Thu)
        calc.CountBusinessDays(new DateOnly(2026, 3, 23), new DateOnly(2026, 3, 26)).Should().Be(3);
    }

    [Fact]
    public void CountBusinessDays_AcrossWeekend()
    {
        var calc = Create();
        // Wed 25 to Mon 30 = 3 business days (Thu, Sun, Mon — Fri+Sat excluded)
        calc.CountBusinessDays(new DateOnly(2026, 3, 25), new DateOnly(2026, 3, 30)).Should().Be(3);
    }

    [Fact]
    public void AddBusinessDays_SkipsFridaySaturday()
    {
        var calc = Create();
        // From Wed 25, add 3 business days = Thu 26, Sun 29, Mon 30
        calc.AddBusinessDays(new DateOnly(2026, 3, 25), 3).Should().Be(new DateOnly(2026, 3, 30));
    }
}
