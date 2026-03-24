using Couture.Clients.Contracts.Dtos;
using Mediator;

namespace Couture.Clients.Features.SearchClients;

public sealed record SearchClientsQuery(string SearchTerm) : IQuery<List<ClientSummaryDto>>;
