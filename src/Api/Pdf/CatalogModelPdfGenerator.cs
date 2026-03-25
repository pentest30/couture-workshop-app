using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Couture.Api.Pdf;

public static class CatalogModelPdfGenerator
{
    public static byte[] Generate(CatalogModelPdfData d)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("L'Atelier Couture — Catalogue").FontSize(16).Bold().FontColor(Colors.Purple.Darken3);
                        row.ConstantItem(120).AlignRight().Text(d.Code).FontSize(12).Bold();
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(6);

                    col.Item().Text(d.Name).FontSize(20).Bold();

                    col.Item().Row(row =>
                    {
                        row.AutoItem().PaddingRight(12).Text(t => { t.Span("Categorie: ").Bold(); t.Span(d.CategoryLabel); });
                        row.AutoItem().PaddingRight(12).Text(t => { t.Span("Type: ").Bold(); t.Span(d.WorkType); });
                        if (d.IsPublic) row.AutoItem().Text("Public").Bold().FontColor(Colors.Green.Darken2);
                    });

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().PaddingVertical(4).Column(c =>
                        {
                            c.Item().Text("Prix de base").FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"{d.BasePrice:N0} DZD").FontSize(16).Bold();
                        });
                        table.Cell().PaddingVertical(4).Column(c =>
                        {
                            c.Item().Text("Duree estimee").FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"{d.EstimatedDays} jours").FontSize(16).Bold();
                        });
                    });

                    if (!string.IsNullOrWhiteSpace(d.Description))
                    {
                        col.Item().PaddingTop(10).Text("Description").Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().Text(d.Description);
                    }

                    if (d.Fabrics.Count > 0)
                    {
                        col.Item().PaddingTop(12).Text("Tissus recommandes").Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); });
                            table.Cell().PaddingVertical(2).Text("Nom").Bold().FontSize(9);
                            table.Cell().PaddingVertical(2).Text("Type").Bold().FontSize(9);
                            table.Cell().PaddingVertical(2).Text("Prix/m").Bold().FontSize(9);
                            foreach (var f in d.Fabrics)
                            {
                                table.Cell().PaddingVertical(2).Text(f.Name);
                                table.Cell().PaddingVertical(2).Text(f.Type);
                                table.Cell().PaddingVertical(2).Text($"{f.PricePerMeter:N0} DZD");
                            }
                        });
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text($"Genere le {DateTime.Now:dd/MM/yyyy a HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                        row.ConstantItem(120).AlignRight().Text("L'Atelier Couture").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        }).GeneratePdf();
    }
}

public sealed record CatalogModelPdfData(
    string Code, string Name, string CategoryLabel, string WorkType,
    decimal BasePrice, int EstimatedDays, bool IsPublic,
    string? Description, List<CatalogFabricPdfItem> Fabrics);

public sealed record CatalogFabricPdfItem(string Name, string Type, decimal PricePerMeter);
