using Couture.Dashboard.Contracts.Dtos;
using Mediator;
namespace Couture.Dashboard.Features.GetMonthlyHistogram;
public sealed record GetMonthlyHistogramQuery(int Year, int Quarter) : IQuery<MonthlyHistogramDto>;
