using Couture.Dashboard.Contracts.Dtos;
using Mediator;
namespace Couture.Dashboard.Features.GetQuarterlyKPIs;
public sealed record GetQuarterlyKPIsQuery(int Year, int Quarter) : IQuery<QuarterlyKPIsDto>;
