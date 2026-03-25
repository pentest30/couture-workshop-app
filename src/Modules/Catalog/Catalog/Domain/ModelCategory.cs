using Ardalis.SmartEnum;

namespace Couture.Catalog.Domain;

public sealed class ModelCategory : SmartEnum<ModelCategory>
{
    public static readonly ModelCategory Ceremonie = new("Ceremonie", 1, "Cérémonie");
    public static readonly ModelCategory Quotidien = new("Quotidien", 2, "Quotidien");
    public static readonly ModelCategory Mariee = new("Mariee", 3, "Mariée");
    public static readonly ModelCategory Traditionnel = new("Traditionnel", 4, "Traditionnel");
    public static readonly ModelCategory Moderne = new("Moderne", 5, "Moderne");

    public string Label { get; }
    private ModelCategory(string name, int value, string label) : base(name, value) => Label = label;
}
