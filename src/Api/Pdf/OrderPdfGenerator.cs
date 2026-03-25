using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Couture.Api.Pdf;

public static class OrderPdfGenerator
{
    public static byte[] Generate(OrderPdfData d)
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
                        row.RelativeItem().Text("L'Atelier Couture").FontSize(18).Bold().FontColor(Colors.Purple.Darken3);
                        row.ConstantItem(140).AlignRight().Column(c =>
                        {
                            c.Item().Text(d.Code).FontSize(14).Bold();
                            c.Item().Text($"Fiche de commande").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(6);

                    // Client + status
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Client").Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(d.ClientName).FontSize(14).Bold();
                        });
                        row.ConstantItem(200).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Statut: {d.StatusLabel}").Bold();
                            c.Item().Text($"Type: {d.WorkTypeLabel}");
                        });
                    });

                    col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten3);

                    // Dates
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                        table.Cell().Text(t => { t.Span("Reception: ").Bold(); t.Span(d.ReceptionDate); });
                        table.Cell().Text(t => { t.Span("Livraison prevue: ").Bold(); t.Span(d.ExpectedDeliveryDate); });
                        if (d.ActualDeliveryDate is not null)
                            table.Cell().Text(t => { t.Span("Livree le: ").Bold(); t.Span(d.ActualDeliveryDate); });
                    });

                    // Description
                    if (!string.IsNullOrWhiteSpace(d.Description))
                    {
                        col.Item().PaddingTop(10).Text("Description").Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().Text(d.Description);
                    }

                    if (!string.IsNullOrWhiteSpace(d.Fabric))
                    {
                        col.Item().PaddingTop(6).Text(t => { t.Span("Tissu: ").Bold(); t.Span(d.Fabric); });
                    }

                    // Embroidery / beading
                    if (!string.IsNullOrWhiteSpace(d.EmbroideryStyle))
                        col.Item().Text(t => { t.Span("Style broderie: ").Bold(); t.Span(d.EmbroideryStyle); });
                    if (!string.IsNullOrWhiteSpace(d.BeadType))
                        col.Item().Text(t => { t.Span("Type de perle: ").Bold(); t.Span(d.BeadType); });

                    if (!string.IsNullOrWhiteSpace(d.TechnicalNotes))
                    {
                        col.Item().PaddingTop(10).Text("Notes techniques").Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().Text(d.TechnicalNotes).FontSize(9);
                    }

                    // Pricing
                    col.Item().PaddingTop(12).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten3);
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });
                        void Row(string label, string value, bool isBold = false)
                        {
                            table.Cell().PaddingVertical(2).Text(label).FontColor(Colors.Grey.Darken2);
                            if (isBold)
                                table.Cell().PaddingVertical(2).AlignRight().Text(value).Bold();
                            else
                                table.Cell().PaddingVertical(2).AlignRight().Text(value);
                        }
                        Row("Prix total", $"{d.TotalPrice:N0} DZD", true);
                        Row("Total paye", $"{d.TotalPaid:N0} DZD");
                        Row("Solde restant", $"{d.Outstanding:N0} DZD", true);
                    });
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

public sealed record OrderPdfData(
    string Code, string ClientName, string StatusLabel, string WorkTypeLabel,
    string ReceptionDate, string ExpectedDeliveryDate, string? ActualDeliveryDate,
    string? Description, string? Fabric, string? TechnicalNotes,
    string? EmbroideryStyle, string? BeadType,
    decimal TotalPrice, decimal TotalPaid, decimal Outstanding);
