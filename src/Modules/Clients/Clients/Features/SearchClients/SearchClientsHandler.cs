using Couture.Clients.Contracts.Dtos;
using Couture.Clients.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Clients.Features.SearchClients;

public sealed class SearchClientsHandler : IQueryHandler<SearchClientsQuery, List<ClientSummaryDto>>
{
    private readonly ClientsDbContext _db;

    public SearchClientsHandler(ClientsDbContext db) => _db = db;

    public async ValueTask<List<ClientSummaryDto>> Handle(SearchClientsQuery query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.SearchTerm) || query.SearchTerm.Trim().Length < 2)
            return [];

        var term = query.SearchTerm.Trim().ToLower();

        var clients = await _db.Clients
            .AsNoTracking()
            .Where(c =>
                c.Code.ToLower().Contains(term) ||
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                c.PrimaryPhone.Contains(term))
            .OrderBy(c => c.LastName)
            .Take(10)
            .Select(c => new ClientSummaryDto(
                c.Id.Value,
                c.Code,
                c.FirstName,
                c.LastName,
                c.FirstName + " " + c.LastName,
                c.PrimaryPhone,
                0)) // ActiveOrderCount requires cross-module query, deferred
            .ToListAsync(ct);

        return clients;
    }
}
