namespace Couture.Dashboard.Domain;

public sealed class WorkshopSettings
{
    public int Id { get; private set; } = 1;
    public string WorkshopName { get; private set; } = "Atelier Couture";
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? LogoPath { get; private set; }
    public int MaxActiveOrdersPerTailor { get; private set; } = 10;

    private WorkshopSettings() { }

    public static WorkshopSettings CreateDefault() => new();

    public void Update(string? name = null, string? address = null, string? phone = null, string? logoPath = null, int? maxOrders = null)
    {
        if (name is not null) WorkshopName = name;
        if (address is not null) Address = address;
        if (phone is not null) Phone = phone;
        if (logoPath is not null) LogoPath = logoPath;
        if (maxOrders.HasValue && maxOrders.Value > 0) MaxActiveOrdersPerTailor = maxOrders.Value;
    }
}
