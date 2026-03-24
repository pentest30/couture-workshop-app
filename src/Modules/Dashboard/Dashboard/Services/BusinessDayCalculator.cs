using Couture.Dashboard.Domain;

namespace Couture.Dashboard.Services;

public sealed class BusinessDayCalculator
{
    private readonly HashSet<DateOnly> _holidays;

    public BusinessDayCalculator(IEnumerable<Holiday> holidays)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _holidays = holidays
            .Select(h => h.IsRecurring ? new DateOnly(today.Year, h.Date.Month, h.Date.Day) : h.Date)
            .ToHashSet();
    }

    public bool IsBusinessDay(DateOnly date)
    {
        if (date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday) return false;
        return !_holidays.Contains(date);
    }

    public int CountBusinessDays(DateOnly from, DateOnly to)
    {
        if (to <= from) return 0;
        int count = 0;
        for (var d = from.AddDays(1); d <= to; d = d.AddDays(1))
        {
            if (IsBusinessDay(d)) count++;
        }
        return count;
    }

    public DateOnly AddBusinessDays(DateOnly from, int days)
    {
        var current = from;
        int added = 0;
        while (added < days)
        {
            current = current.AddDays(1);
            if (IsBusinessDay(current)) added++;
        }
        return current;
    }

    public int GetDelayBusinessDays(DateOnly expectedDelivery)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today <= expectedDelivery) return 0;
        return CountBusinessDays(expectedDelivery, today);
    }
}
