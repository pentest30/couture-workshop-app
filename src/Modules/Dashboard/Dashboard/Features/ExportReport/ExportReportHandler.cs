using Couture.Orders.Domain;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Dashboard.Features.ExportReport;

public sealed class ExportReportHandler : IQueryHandler<ExportReportQuery, ExportResult>
{
    private readonly OrdersDbContext _db;
    public ExportReportHandler(OrdersDbContext db) => _db = db;

    public async ValueTask<ExportResult> Handle(ExportReportQuery query, CancellationToken ct)
    {
        var startMonth = (query.Quarter - 1) * 3 + 1;
        var start = new DateOnly(query.Year, startMonth, 1);
        var end = start.AddMonths(3).AddDays(-1);

        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.ReceptionDate >= start && o.ReceptionDate <= end)
            .OrderBy(o => o.Code)
            .ToListAsync(ct);

        return query.Format.ToLower() switch
        {
            "csv" => GenerateCsv(orders, query.Year, query.Quarter),
            _ => GenerateCsv(orders, query.Year, query.Quarter), // Default to CSV; XLSX/PDF can be added later
        };
    }

    private static ExportResult GenerateCsv(List<Order> orders, int year, int quarter)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Code,Client,Type,Statut,Date Livraison,Prix Total,Retard");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var o in orders)
        {
            var isLate = o.Status != OrderStatus.Livree && o.ExpectedDeliveryDate < today;
            var delay = isLate ? (today.DayNumber - o.ExpectedDeliveryDate.DayNumber) : 0;
            sb.AppendLine($"{o.Code},{o.ClientId},{o.WorkType.Label},{o.Status.Label},{o.ExpectedDeliveryDate:dd/MM/yyyy},{o.TotalPrice},{delay}");
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return new ExportResult(bytes, "text/csv", $"rapport-T{quarter}-{year}.csv");
    }
}
