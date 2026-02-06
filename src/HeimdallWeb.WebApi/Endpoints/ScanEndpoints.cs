using System.Security.Claims;
using HeimdallWeb.Application.Commands.Scan.ExecuteScan;
using HeimdallWeb.Application.Queries.Scan.GetUserScanHistories;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.WebApi.Endpoints;

public static class ScanEndpoints
{
    public static RouteGroupBuilder MapScanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/scans")
            .WithTags("Scans")
            .RequireAuthorization();

        group.MapPost("", ExecuteScan)
            .RequireRateLimiting("ScanPolicy");

        group.MapGet("", GetUserScans);

        return group;
    }

    private static async Task<IResult> ExecuteScan(
        [FromBody] ExecuteScanRequest request,
        ICommandHandler<ExecuteScanCommand, ExecuteScanResponse> handler,
        HttpContext context)
    {
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Create command with userId from JWT token and remote IP
        var command = new ExecuteScanCommand(request.Target, userId, remoteIp);
        var result = await handler.Handle(command);

        // Return 201 Created with location header
        return Results.Created($"/api/v1/scan-histories/{result.HistoryId}", result);
    }

    private static async Task<IResult> GetUserScans(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IQueryHandler<GetUserScanHistoriesQuery, PaginatedScanHistoriesResponse> handler,
        HttpContext context)
    {
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Default pagination values
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 10;

        var query = new GetUserScanHistoriesQuery(userId, page, pageSize);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }
}
