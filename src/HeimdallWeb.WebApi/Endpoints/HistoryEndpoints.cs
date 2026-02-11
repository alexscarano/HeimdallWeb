using System.Security.Claims;
using HeimdallWeb.Application.Commands.Scan.DeleteScanHistory;
using HeimdallWeb.Application.Queries.Scan.GetScanHistoryById;
using HeimdallWeb.Application.Queries.Scan.GetFindingsByHistoryId;
using HeimdallWeb.Application.Queries.Scan.GetTechnologiesByHistoryId;
using HeimdallWeb.Application.Queries.Scan.GetAISummaryByHistoryId;
using HeimdallWeb.Application.Queries.Scan.ExportSingleHistoryPdf;
using HeimdallWeb.Application.Queries.Scan.ExportHistoryPdf;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.WebApi.Endpoints;

public static class HistoryEndpoints
{
    public static RouteGroupBuilder MapHistoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/scan-histories")
            .WithTags("Scan History")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetScanHistoryById);
        group.MapGet("/{id:guid}/findings", GetFindings);
        group.MapGet("/{id:guid}/technologies", GetTechnologies);
        group.MapGet("/{id:guid}/ai-summary", GetAISummary);
        group.MapGet("/{id:guid}/export", ExportSinglePdf);
        group.MapGet("/export", ExportAllPdf);
        group.MapDelete("/{id:guid}", DeleteScanHistory);

        return group;
    }

    private static async Task<IResult> GetScanHistoryById(
        Guid id,
        IQueryHandler<GetScanHistoryByIdQuery, ScanHistoryDetailResponse> handler,
        HttpContext context)
    {
        var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        var query = new GetScanHistoryByIdQuery(id, userId);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetFindings(
        Guid id,
        IQueryHandler<GetFindingsByHistoryIdQuery, IEnumerable<FindingResponse>> handler,
        HttpContext context)
    {
        var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        var query = new GetFindingsByHistoryIdQuery(id, userId);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetTechnologies(
        Guid id,
        IQueryHandler<GetTechnologiesByHistoryIdQuery, IEnumerable<TechnologyResponse>> handler,
        HttpContext context)
    {
        var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        var query = new GetTechnologiesByHistoryIdQuery(id, userId);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetAISummary(
        Guid id,
        IQueryHandler<GetAISummaryByHistoryIdQuery, IASummaryResponse?> handler,
        HttpContext context)
    {
        var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        var query = new GetAISummaryByHistoryIdQuery(id, userId);
        var result = await handler.Handle(query);

        // Return 404 if no AI summary exists for this scan
        if (result == null)
            return Results.NotFound(new { message = "AI summary not available for this scan" });

        return Results.Ok(result);
    }

    private static async Task<IResult> ExportSinglePdf(
        Guid id,
        IQueryHandler<ExportSingleHistoryPdfQuery, PdfExportResponse> handler,
        HttpContext context)
    {
        var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var username = context.User.Identity?.Name ?? "Anonymous";

        var query = new ExportSingleHistoryPdfQuery(id, userId, username);
        var result = await handler.Handle(query);

        return Results.File(
            result.PdfData,
            contentType: result.ContentType,
            fileDownloadName: result.FileName);
    }

    private static async Task<IResult> ExportAllPdf(
        IQueryHandler<ExportHistoryPdfQuery, PdfExportResponse> handler,
        HttpContext context)
    {
        var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var username = context.User.Identity?.Name ?? "Anonymous";

        var query = new ExportHistoryPdfQuery(userId, username);
        var result = await handler.Handle(query);

        return Results.File(
            result.PdfData,
            contentType: result.ContentType,
            fileDownloadName: result.FileName);
    }

    private static async Task<IResult> DeleteScanHistory(
        Guid id,
        ICommandHandler<DeleteScanHistoryCommand, DeleteScanHistoryResponse> handler,
        HttpContext context)
    {
        var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        var command = new DeleteScanHistoryCommand(id, userId);
        await handler.Handle(command);

        return Results.Ok(new { message = "Scan history deleted successfully", historyId = id });
    }
}
