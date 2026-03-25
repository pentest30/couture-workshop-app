using Couture.Dashboard.Contracts.Dtos;
using Mediator;
namespace Couture.Dashboard.Features.GetRevenueTrend;
public sealed record GetRevenueTrendQuery(int Year, int Quarter) : IQuery<RevenueTrendDto>;
