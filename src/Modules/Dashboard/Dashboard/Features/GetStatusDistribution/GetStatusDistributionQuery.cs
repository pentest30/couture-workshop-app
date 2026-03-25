using Couture.Dashboard.Contracts.Dtos;
using Mediator;
namespace Couture.Dashboard.Features.GetStatusDistribution;
public sealed record GetStatusDistributionQuery(int Year, int Quarter) : IQuery<StatusDistributionDto>;
