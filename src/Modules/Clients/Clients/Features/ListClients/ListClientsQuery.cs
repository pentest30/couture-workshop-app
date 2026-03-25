using Couture.Clients.Contracts.Dtos;
using Mediator;
namespace Couture.Clients.Features.ListClients;
public sealed record ListClientsQuery(string? Search, int Page = 1, int PageSize = 20) : IQuery<ListClientsResult>;
public sealed record ListClientsResult(List<ClientSummaryDto> Items, int TotalCount);
