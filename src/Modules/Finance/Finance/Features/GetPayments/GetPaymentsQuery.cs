using Couture.Finance.Contracts.Dtos;
using Mediator;

namespace Couture.Finance.Features.GetPayments;

public sealed record GetPaymentsQuery(Guid OrderId) : IQuery<List<PaymentDto>>;
