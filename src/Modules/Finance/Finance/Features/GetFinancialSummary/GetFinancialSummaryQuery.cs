using Couture.Finance.Contracts.Dtos;
using Mediator;

namespace Couture.Finance.Features.GetFinancialSummary;

public sealed record GetFinancialSummaryQuery(int Year, int Quarter) : IQuery<FinancialSummaryDto>;
