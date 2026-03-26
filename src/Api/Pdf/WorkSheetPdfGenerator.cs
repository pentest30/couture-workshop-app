using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Couture.Api.Pdf;

public static class WorkSheetPdfGenerator
{
    public static byte[] Generate(WorkSheetData d)
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
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("L'Atelier Couture").FontSize(16).Bold().FontColor(Colors.Purple.Darken3);
                            c.Item().Text("FICHE DE TRAVAIL — ARTISAN").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                        row.ConstantItem(140).AlignRight().Column(c =>
                        {
                            c.Item().Text(d.OrderCode).FontSize(14).Bold();
                            c.Item().Text($"Livraison: {d.ExpectedDeliveryDate}").FontSize(10).Bold().FontColor(Colors.Red.Darken2);
                        });
                    });
                    col.Item().PaddingTop(8).LineHorizontal(2).LineColor(Colors.Purple.Darken3);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(4);

                    // === CLIENT SECTION ===
                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(section =>
                    {
                        section.Item().Text("CLIENT").FontSize(9).Bold().FontColor(Colors.Purple.Darken3);
                        section.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(d.ClientName).FontSize(14).Bold();
                                c.Item().Text($"Tel: {d.ClientPhone}").FontSize(10);
                            });
                            row.ConstantItem(150).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Code: {d.ClientCode}");
                                if (!string.IsNullOrWhiteSpace(d.ClientAddress))
                                    c.Item().Text(d.ClientAddress).FontSize(9);
                            });
                        });
                    });

                    // === MEASUREMENTS ===
                    if (d.Measurements.Count > 0)
                    {
                        col.Item().PaddingTop(8).Text("MENSURATIONS").FontSize(9).Bold().FontColor(Colors.Purple.Darken3);
                        col.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                for (int i = 0; i < 5; i++) { c.RelativeColumn(); c.ConstantColumn(50); }
                            });

                            for (int i = 0; i < d.Measurements.Count; i++)
                            {
                                var m = d.Measurements[i];
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4)
                                    .Text(m.Name).FontSize(9).FontColor(Colors.Grey.Darken2);
                                table.Cell().PaddingVertical(3).PaddingHorizontal(4)
                                    .AlignRight().Text($"{m.Value} {m.Unit}").FontSize(10).Bold();
                            }
                            // Fill remaining cells if not multiple of 5
                            var remainder = d.Measurements.Count % 5;
                            if (remainder > 0)
                                for (int i = 0; i < (5 - remainder) * 2; i++)
                                    table.Cell();
                        });
                    }

                    // === ORDER DETAILS ===
                    col.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(8).Text("DETAILS DE LA COMMANDE").FontSize(9).Bold().FontColor(Colors.Purple.Darken3);

                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                        void Cell(string label, string value)
                        {
                            table.Cell().PaddingVertical(3).Column(c =>
                            {
                                c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(value).FontSize(10).Bold();
                            });
                        }
                        Cell("Type de travail", d.WorkType);
                        Cell("Statut", d.Status);
                        Cell("Reception", d.ReceptionDate);
                        Cell("Prix", $"{d.TotalPrice:N0} DZD");
                    });

                    // Descriptione
                    if (!string.IsNullOrWhiteSpace(d.Description))
                    {
                        col.Item().PaddingTop(8).Text("DESCRIPTION").FontSize(9).Bold().FontColor(Colors.Purple.Darken3);
                        col.Item().PaddingTop(2).Text(d.Description).FontSize(10);
                    }

                    // Fabric
                    if (!string.IsNullOrWhiteSpace(d.Fabric))
                    {
                        col.Item().PaddingTop(6).Text(t => { t.Span("Tissu: ").Bold(); t.Span(d.Fabric); });
                    }

                    // Embroidery
                    if (!string.IsNullOrWhiteSpace(d.EmbroideryStyle) || !string.IsNullOrWhiteSpace(d.ThreadColors) ||
                        !string.IsNullOrWhiteSpace(d.Density) || !string.IsNullOrWhiteSpace(d.EmbroideryZone))
                    {
                        col.Item().PaddingTop(8).Text("BRODERIE").FontSize(9).Bold().FontColor(Colors.Purple.Darken3);
                        col.Item().PaddingTop(2).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            if (!string.IsNullOrWhiteSpace(d.EmbroideryStyle))
                            { table.Cell().Text(t => { t.Span("Style: ").Bold(); t.Span(d.EmbroideryStyle); }); }
                            if (!string.IsNullOrWhiteSpace(d.ThreadColors))
                            { table.Cell().Text(t => { t.Span("Couleurs fils: ").Bold(); t.Span(d.ThreadColors); }); }
                            if (!string.IsNullOrWhiteSpace(d.Density))
                            { table.Cell().Text(t => { t.Span("Densite: ").Bold(); t.Span(d.Density); }); }
                            if (!string.IsNullOrWhiteSpace(d.EmbroideryZone))
                            { table.Cell().Text(t => { t.Span("Zone: ").Bold(); t.Span(d.EmbroideryZone); }); }
                        });
                    }

                    // Beading
                    if (!string.IsNullOrWhiteSpace(d.BeadType) || !string.IsNullOrWhiteSpace(d.Arrangement) || !string.IsNullOrWhiteSpace(d.AffectedZones))
                    {
                        col.Item().PaddingTop(8).Text("PERLAGE").FontSize(9).Bold().FontColor(Colors.Purple.Darken3);
                        col.Item().PaddingTop(2).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            if (!string.IsNullOrWhiteSpace(d.BeadType))
                            { table.Cell().Text(t => { t.Span("Type: ").Bold(); t.Span(d.BeadType); }); }
                            if (!string.IsNullOrWhiteSpace(d.Arrangement))
                            { table.Cell().Text(t => { t.Span("Arrangement: ").Bold(); t.Span(d.Arrangement); }); }
                            if (!string.IsNullOrWhiteSpace(d.AffectedZones))
                            { table.Cell().Text(t => { t.Span("Zones: ").Bold(); t.Span(d.AffectedZones); }); }
                        });
                    }

                    // Technical notes
                    if (!string.IsNullOrWhiteSpace(d.TechnicalNotes))
                    {
                        col.Item().PaddingTop(8).Text("NOTES TECHNIQUES").FontSize(9).Bold().FontColor(Colors.Purple.Darken3);
                        col.Item().PaddingTop(2).Background(Colors.Yellow.Lighten4).Padding(8)
                            .Text(d.TechnicalNotes).FontSize(10);
                    }

                    // Catalog model
                    if (!string.IsNullOrWhiteSpace(d.CatalogModelName))
                    {
                        col.Item().PaddingTop(8).Text("MODELE CATALOGUE").FontSize(9).Bold().FontColor(Colors.Purple.Darken3);
                        col.Item().PaddingTop(2).Text(t =>
                        {
                            t.Span($"{d.CatalogModelCode} — ").Bold();
                            t.Span(d.CatalogModelName);
                        });
                        if (!string.IsNullOrWhiteSpace(d.CatalogModelDescription))
                            col.Item().PaddingTop(2).Text(d.CatalogModelDescription).FontSize(9).FontColor(Colors.Grey.Darken1);

                        if (d.CatalogFabrics.Count > 0)
                        {
                            col.Item().PaddingTop(4).Text("Tissus recommandes:").FontSize(9).Bold();
                            foreach (var f in d.CatalogFabrics)
                                col.Item().Text($"  - {f}").FontSize(9);
                        }
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(2).LineColor(Colors.Purple.Darken3);
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text($"Fiche generee le {DateTime.Now:dd/MM/yyyy a HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                        row.ConstantItem(200).AlignRight().Text("CONFIDENTIEL — Usage interne artisan").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        }).GeneratePdf();
    }
}

public sealed record WorkSheetData(
    string OrderCode, string ClientName, string ClientCode, string ClientPhone, string? ClientAddress,
    string WorkType, string Status, string ReceptionDate, string ExpectedDeliveryDate,
    decimal TotalPrice,
    string? Description, string? Fabric, string? TechnicalNotes,
    string? EmbroideryStyle, string? ThreadColors, string? Density, string? EmbroideryZone,
    string? BeadType, string? Arrangement, string? AffectedZones,
    string? CatalogModelCode, string? CatalogModelName, string? CatalogModelDescription,
    List<MeasurementItem> Measurements, List<string> CatalogFabrics);

public sealed record MeasurementItem(string Name, decimal Value, string Unit);
