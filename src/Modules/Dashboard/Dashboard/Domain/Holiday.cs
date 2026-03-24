namespace Couture.Dashboard.Domain;

public sealed class Holiday
{
    public int Id { get; private set; }
    public DateOnly Date { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsRecurring { get; private set; }

    private Holiday() { }

    public static Holiday Create(DateOnly date, string name, bool isRecurring)
    {
        return new Holiday { Date = date, Name = name, IsRecurring = isRecurring };
    }
}
