using Couture.Clients.Contracts.Dtos;
using Couture.Clients.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace Couture.Clients.Features.ListClients;
public sealed class ListClientsHandler : IQueryHandler<ListClientsQuery, ListClientsResult>
{
    private readonly ClientsDbContext _db;
    public ListClientsHandler(ClientsDbContext db) => _db = db;
    public async ValueTask<ListClientsResult> Handle(ListClientsQuery query, CancellationToken ct)
    {
        var q = _db.Clients.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(c => c.Code.ToLower().Contains(term) || c.FirstName.ToLower().Contains(term) || c.LastName.ToLower().Contains(term) || c.PrimaryPhone.Contains(term));
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(c => new ClientSummaryDto(c.Id.Value, c.Code, c.FirstName, c.LastName, c.FirstName + " " + c.LastName, c.PrimaryPhone, 0))
            .ToListAsync(ct);
        return new ListClientsResult(items, total);
    }
}
