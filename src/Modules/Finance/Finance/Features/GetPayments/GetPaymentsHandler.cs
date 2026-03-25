using Couture.Finance.Contracts.Dtos;
using Couture.Finance.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Couture.Finance.Features.GetPayments;

public sealed class GetPaymentsHandler : IQueryHandler<GetPaymentsQuery, List<PaymentDto>>
{
    private readonly FinanceDbContext _db;

    public GetPaymentsHandler(FinanceDbContext db) => _db = db;

    public async ValueTask<List<PaymentDto>> Handle(GetPaymentsQuery query, CancellationToken ct)
    {
        return await _db.Payments
            .AsNoTracking()
            .Include(p => p.Receipt)
            .Where(p => p.OrderId == query.OrderId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentDto(
                p.Id.Value,
                p.OrderId,
                p.Amount,
                p.PaymentMethod.Name,
                p.PaymentMethod.Label,
                p.PaymentDate,
                p.Note,
                p.Receipt != null ? p.Receipt.Code : null,
                p.RecordedBy,
                p.CreatedAt))
            .ToListAsync(ct);
    }
}
