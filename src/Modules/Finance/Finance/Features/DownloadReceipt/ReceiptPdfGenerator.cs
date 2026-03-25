using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Couture.Finance.Features.DownloadReceipt;

public static class ReceiptPdfGenerator
{
    public static byte[] Generate(ReceiptPdfData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("L'Atelier Couture")
                            .FontSize(18).Bold().FontColor(Colors.Purple.Darken3);
                        row.ConstantItem(100).AlignRight().Text(data.ReceiptCode)
                            .FontSize(12).Bold();
                    });
                    col.Item().PaddingTop(4).Text("Recu de paiement")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(8);

                    // Order info
                    col.Item().Text(text =>
                    {
                        text.Span("Commande: ").Bold();
                        text.Span(data.OrderCode);
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("Client: ").Bold();
                        text.Span(data.ClientName);
                    });

                    col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten3);
                    col.Item().PaddingTop(8);

                    // Payment details table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                        });

                        void AddRow(string label, string value)
                        {
                            table.Cell().PaddingVertical(3).Text(label).FontColor(Colors.Grey.Darken2);
                            table.Cell().PaddingVertical(3).AlignRight().Text(value).Bold();
                        }

                        AddRow("Montant", $"{data.Amount:N0} DZD");
                        AddRow("Mode de paiement", data.PaymentMethodLabel);
                        AddRow("Date de paiement", data.PaymentDate.ToString("dd/MM/yyyy"));
                        AddRow("Prix total commande", $"{data.OrderTotalPrice:N0} DZD");
                        AddRow("Total paye", $"{data.TotalPaid:N0} DZD");
                        AddRow("Solde restant", $"{data.Outstanding:N0} DZD");
                    });

                    if (!string.IsNullOrWhiteSpace(data.Note))
                    {
                        col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten3);
                        col.Item().PaddingTop(6).Text(text =>
                        {
                            text.Span("Note: ").Bold().FontSize(9);
                            text.Span(data.Note).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text($"Genere le {data.GeneratedAt:dd/MM/yyyy a HH:mm}")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                        row.ConstantItem(120).AlignRight().Text("L'Atelier Couture")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        }).GeneratePdf();
    }
}

public sealed record ReceiptPdfData(
    string ReceiptCode,
    string OrderCode,
    string ClientName,
    decimal Amount,
    string PaymentMethodLabel,
    DateOnly PaymentDate,
    string? Note,
    decimal OrderTotalPrice,
    decimal TotalPaid,
    decimal Outstanding,
    DateTimeOffset GeneratedAt);
