using System.Security.Claims;
using HeimdallWeb.Application.Commands.Monitor.CreateMonitor;
using HeimdallWeb.Application.Commands.Monitor.DeleteMonitor;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Monitor;
using HeimdallWeb.Application.Queries.Monitor.GetMonitorHistory;
using HeimdallWeb.Application.Queries.Monitor.GetUserMonitors;
using HeimdallWeb.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.WebApi.Endpoints;

/// <summary>
/// Minimal API endpoints for the monitored targets feature (Sprint 4).
/// Allows authenticated users to register, list, and delete monitored URLs,
/// and to view their risk snapshot history.
/// </summary>
public static class MonitorEndpoints
{
    /// <summary>
    /// Maps all monitor-related endpoints under <c>/api/v1/monitor</c>.
    /// </summary>
    public static RouteGroupBuilder MapMonitorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/monitor")
            .WithTags("Monitor")
            .RequireAuthorization();

        group.MapGet("", GetUserMonitors)
            .WithSummary("List all monitored targets for the authenticated user.");

        group.MapPost("", CreateMonitor)
            .WithSummary("Register a new URL to monitor periodically.");

        group.MapDelete("{id:int}", DeleteMonitor)
            .WithSummary("Remove a monitored target.");

        group.MapGet("{id:int}/history", GetMonitorHistory)
            .WithSummary("Retrieve the risk snapshot history for a monitored target (last 30 entries).");

        return group;
    }

    private static async Task<IResult> GetUserMonitors(
        IQueryHandler<GetUserMonitorsQuery, IEnumerable<MonitoredTargetResponse>> handler,
        HttpContext context)
    {
        var userId = GetUserId(context);
        var result = await handler.Handle(new GetUserMonitorsQuery(userId));
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateMonitor(
        [FromBody] CreateMonitorRequest request,
        ICommandHandler<CreateMonitorCommand, MonitoredTargetResponse> handler,
        HttpContext context)
    {
        var userId = GetUserId(context);

        if (!Enum.TryParse<MonitorFrequency>(request.Frequency, ignoreCase: true, out var frequency))
            return Results.BadRequest(new { error = "Invalid frequency. Use 'Daily' or 'Weekly'." });

        var command = new CreateMonitorCommand(userId, request.Url, frequency);
        var result = await handler.Handle(command);

        return Results.Created($"/api/v1/monitor/{result.Id}", result);
    }

    private static async Task<IResult> DeleteMonitor(
        int id,
        ICommandHandler<DeleteMonitorCommand, bool> handler,
        HttpContext context)
    {
        var userId = GetUserId(context);
        await handler.Handle(new DeleteMonitorCommand(id, userId));
        return Results.NoContent();
    }

    private static async Task<IResult> GetMonitorHistory(
        int id,
        IQueryHandler<GetMonitorHistoryQuery, IEnumerable<RiskSnapshotResponse>> handler,
        HttpContext context)
    {
        var userId = GetUserId(context);
        var result = await handler.Handle(new GetMonitorHistoryQuery(id, userId));
        return Results.Ok(result);
    }

    /// <summary>
    /// Extracts the user's public UUID from the JWT <c>NameIdentifier</c> claim.
    /// Throws <see cref="UnauthorizedAccessException"/> if the claim is absent or malformed,
    /// ensuring a proper 401 response instead of proceeding with Guid.Empty.
    /// </summary>
    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(claim) || !Guid.TryParse(claim, out var userId) || userId == Guid.Empty)
            throw new UnauthorizedAccessException("Invalid or missing user identity claim.");

        return userId;
    }
}
