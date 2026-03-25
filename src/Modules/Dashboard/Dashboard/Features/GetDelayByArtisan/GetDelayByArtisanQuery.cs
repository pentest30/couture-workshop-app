using Couture.Dashboard.Contracts.Dtos;
using Mediator;
namespace Couture.Dashboard.Features.GetDelayByArtisan;
public sealed record GetDelayByArtisanQuery(int Year, int Quarter) : IQuery<DelayByArtisanDto>;
