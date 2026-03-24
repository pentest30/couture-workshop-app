using Ardalis.SmartEnum;
namespace Couture.Finance.Domain;

public sealed class PaymentMethod : SmartEnum<PaymentMethod>
{
    public static readonly PaymentMethod Especes = new(nameof(Especes), 1, "Esp\u00e8ces");
    public static readonly PaymentMethod Virement = new(nameof(Virement), 2, "Virement");
    public static readonly PaymentMethod Ccp = new(nameof(Ccp), 3, "CCP");
    public static readonly PaymentMethod BaridiMob = new(nameof(BaridiMob), 4, "BaridiMob");
    public static readonly PaymentMethod Dahabia = new(nameof(Dahabia), 5, "Dahabia");

    public string Label { get; }
    private PaymentMethod(string name, int value, string label) : base(name, value) => Label = label;
}
