using Couture.Finance.Features.GetFinancialSummary;
using Couture.Finance.Features.GetPayments;
using Couture.Finance.Features.RecordPayment;
using Couture.Identity.Contracts;
using Mediator;
using Microsoft.AspNetCore.Mvc;

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
