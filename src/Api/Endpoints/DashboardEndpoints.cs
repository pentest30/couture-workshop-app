using Couture.Dashboard.Features.GetDelayByArtisan;
using Couture.Dashboard.Features.GetMonthlyHistogram;
using Couture.Dashboard.Features.GetQuarterlyKPIs;
using Couture.Dashboard.Features.GetRevenueTrend;
using Couture.Dashboard.Features.GetStatusDistribution;
using Couture.Identity.Contracts;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Couture.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard")
            .RequireAuthorization(CouturePermissions.DashboardView);

        group.MapGet("/kpis", GetKPIs);
        group.MapGet("/charts/monthly-histogram", GetMonthlyHistogram);
        group.MapGet("/charts/status-distribution", GetStatusDistribution);
        group.MapGet("/charts/revenue-trend", GetRevenueTrend);
        group.MapGet("/charts/delay-by-artisan", GetDelayByArtisan);
    }

    private static async Task<IResult> GetKPIs([FromQuery] int year, [FromQuery] int quarter, IMediator mediator)
    {
        if (quarter < 1 || quarter > 4) return Results.BadRequest(new { error = "Quarter must be 1-4." });
        return Results.Ok(await mediator.Send(new GetQuarterlyKPIsQuery(year, quarter)));
    }

    private static async Task<IResult> GetMonthlyHistogram([FromQuery] int year, [FromQuery] int quarter, IMediator mediator)
        => Results.Ok(await mediator.Send(new GetMonthlyHistogramQuery(year, quarter)));

    private static async Task<IResult> GetStatusDistribution([FromQuery] int year, [FromQuery] int quarter, IMediator mediator)
        => Results.Ok(await mediator.Send(new GetStatusDistributionQuery(year, quarter)));

    private static async Task<IResult> GetRevenueTrend([FromQuery] int year, [FromQuery] int quarter, IMediator mediator)
        => Results.Ok(await mediator.Send(new GetRevenueTrendQuery(year, quarter)));

    private static async Task<IResult> GetDelayByArtisan([FromQuery] int year, [FromQuery] int quarter, IMediator mediator)
        => Results.Ok(await mediator.Send(new GetDelayByArtisanQuery(year, quarter)));
}
