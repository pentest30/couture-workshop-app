using Ardalis.SmartEnum;

namespace Couture.Orders.Domain;

public sealed class OrderStatus : SmartEnum<OrderStatus>
{
    public static readonly OrderStatus Recue = new(nameof(Recue), 1, "Reçue", "#1565C0");
    public static readonly OrderStatus EnAttente = new(nameof(EnAttente), 2, "En Attente", "#F9A825");
    public static readonly OrderStatus EnCours = new(nameof(EnCours), 3, "En Cours", "#E65100");
    public static readonly OrderStatus Broderie = new(nameof(Broderie), 4, "En Broderie", "#6A1B9A");
    public static readonly OrderStatus Perlage = new(nameof(Perlage), 5, "En Perlage", "#880E4F");
    public static readonly OrderStatus Retouche = new(nameof(Retouche), 6, "En Retouche", "#C62828");
    public static readonly OrderStatus Prete = new(nameof(Prete), 7, "Prête", "#2E7D32");
    public static readonly OrderStatus Livree = new(nameof(Livree), 8, "Livrée", "#424242");

    public string Label { get; }
    public string Color { get; }

    private OrderStatus(string name, int value, string label, string color) : base(name, value)
    {
        Label = label;
        Color = color;
    }

    public bool IsTerminal => this == Livree;

    public bool CanTransitionTo(OrderStatus target, WorkType workType)
    {
        return (this, target) switch
        {
            (_, _) when this == target => false,
            var (from, to) when from == Recue && to == EnAttente => true,
            var (from, to) when from == Recue && to == EnCours => true,
            var (from, to) when from == EnAttente && to == EnCours => true,
            var (from, to) when from == EnCours && to == Broderie => workType == WorkType.Brode || workType == WorkType.Mixte,
            var (from, to) when from == EnCours && to == Perlage => workType == WorkType.Perle || workType == WorkType.Mixte,
            var (from, to) when from == EnCours && to == Retouche => true,
            var (from, to) when from == EnCours && to == Prete => true,
            var (from, to) when from == Broderie && to == Perlage => workType == WorkType.Mixte,
            var (from, to) when from == Broderie && to == Retouche => true,
            var (from, to) when from == Broderie && to == Prete => true,
            var (from, to) when from == Perlage && to == Retouche => true,
            var (from, to) when from == Perlage && to == Prete => true,
            var (from, to) when from == Retouche && to == EnCours => true,
            var (from, to) when from == Retouche && to == Broderie => workType == WorkType.Brode || workType == WorkType.Mixte,
            var (from, to) when from == Retouche && to == Perlage => workType == WorkType.Perle || workType == WorkType.Mixte,
            var (from, to) when from == Retouche && to == Prete => true,
            var (from, to) when from == Prete && to == Livree => true,
            _ => false,
        };
    }
}
