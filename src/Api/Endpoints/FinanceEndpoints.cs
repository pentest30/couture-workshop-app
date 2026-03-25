using Couture.Clients.Persistence;
using Couture.Finance.Features.DownloadReceipt;
using Couture.Finance.Features.GetFinancialSummary;
using Couture.Finance.Features.GetPayments;
using Couture.Finance.Features.RecordPayment;
using Couture.Finance.Persistence;
using Couture.Identity.Contracts;
using Couture.Orders.Persistence;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Couture.Api.Endpoints;

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Finance").RequireAuthorization();

        group.MapPost("/orders/{orderId:guid}/payments", RecordPayment)
            .RequireAuthorization(CouturePermissions.FinanceRecord);

        group.MapGet("/orders/{orderId:guid}/payments", GetPayments)
            .RequireAuthorization(CouturePermissions.FinanceView);

        group.MapGet("/finance/receipts/{paymentId:guid}/pdf", DownloadReceiptPdf)
            .AllowAnonymous();

        group.MapGet("/finance/summary", GetFinancialSummary)
            .RequireAuthorization(CouturePermissions.FinanceView);
    }

    private static async Task<IResult> RecordPayment(
        Guid orderId,
        [FromBody] RecordPaymentRequest request,
        IMediator mediator,
        ICurrentUser currentUser)
    {
        try
        {
            var command = new RecordPaymentCommand(
                orderId, request.Amount, request.PaymentMethod,
                request.PaymentDate, request.Note, currentUser.UserId);
            var result = await mediator.Send(command);
            return Results.Created($"/api/orders/{orderId}/payments/{result.PaymentId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetPayments(Guid orderId, IMediator mediator)
    {
        var result = await mediator.Send(new GetPaymentsQuery(orderId));
        return Results.Ok(result);
    }

    private static async Task<IResult> DownloadReceiptPdf(
        Guid paymentId,
        FinanceDbContext financeDb,
        OrdersDbContext ordersDb,
        ClientsDbContext clientsDb)
    {
        var pid = Couture.Finance.Contracts.PaymentId.From(paymentId);
        var payment = await financeDb.Payments
            .AsNoTracking()
            .Include(p => p.Receipt)
            .FirstOrDefaultAsync(p => p.Id == pid);

        if (payment?.Receipt is null)
            return Results.NotFound(new { error = "Receipt not found." });

        var oid = Couture.Orders.Contracts.OrderId.From(payment.OrderId);
        var order = await ordersDb.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == oid);

        if (order is null)
            return Results.NotFound(new { error = "Order not found." });

        var cid = Couture.Clients.Contracts.ClientId.From(order.ClientId);
        var client = await clientsDb.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cid);

        var totalPaid = await financeDb.Payments
            .Where(p => p.OrderId == payment.OrderId)
            .SumAsync(p => p.Amount);

        var pdfData = new ReceiptPdfData(
            payment.Receipt.Code,
            order.Code,
            client is not null ? $"{client.FirstName} {client.LastName}" : "—",
            payment.Amount,
            payment.PaymentMethod.Label,
            payment.PaymentDate,
            payment.Note,
            order.TotalPrice,
            totalPaid,
            order.TotalPrice - totalPaid,
            payment.Receipt.GeneratedAt);

        var pdfBytes = ReceiptPdfGenerator.Generate(pdfData);

        return Results.File(pdfBytes, "application/pdf", $"{payment.Receipt.Code}.pdf");
    }

    private static async Task<IResult> GetFinancialSummary(
        [FromQuery] int year,
        [FromQuery] int quarter,
        IMediator mediator)
    {
        if (quarter < 1 || quarter > 4)
            return Results.BadRequest(new { error = "Quarter must be between 1 and 4." });

        var result = await mediator.Send(new GetFinancialSummaryQuery(year, quarter));
        return Results.Ok(result);
    }
}

public record RecordPaymentRequest(decimal Amount, string PaymentMethod, DateOnly PaymentDate, string? Note);
