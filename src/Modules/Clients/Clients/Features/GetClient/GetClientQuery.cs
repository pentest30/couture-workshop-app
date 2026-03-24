using Couture.Clients.Contracts.Dtos;
using Mediator;

namespace Couture.Clients.Features.GetClient;

public sealed record GetClientQuery(Guid ClientId) : IQuery<ClientDetailDto?>;
