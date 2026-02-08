using System.Security.Claims;
using HeimdallWeb.Application.Commands.Scan.DeleteScanHistory;
using HeimdallWeb.Application.Queries.Scan.GetScanHistoryById;
using HeimdallWeb.Application.Queries.Scan.GetFindingsByHistoryId;
using HeimdallWeb.Application.Queries.Scan.GetTechnologiesByHistoryId;
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

        group.MapGet("/{id:int}", GetScanHistoryById);
        group.MapGet("/{id:int}/findings", GetFindings);
        group.MapGet("/{id:int}/technologies", GetTechnologies);
        group.MapGet("/{id:int}/export", ExportSinglePdf);
        group.MapGet("/export", ExportAllPdf);
        group.MapDelete("/{id:int}", DeleteScanHistory);

        return group;
    }

    private static async Task<IResult> GetScanHistoryById(
        int id,
        IQueryHandler<GetScanHistoryByIdQuery, ScanHistoryDetailResponse> handler,
        HttpContext context)
    {
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var query = new GetScanHistoryByIdQuery(id, userId);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetFindings(
        int id,
        IQueryHandler<GetFindingsByHistoryIdQuery, IEnumerable<FindingResponse>> handler,
        HttpContext context)
    {
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var query = new GetFindingsByHistoryIdQuery(id, userId);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetTechnologies(
        int id,
        IQueryHandler<GetTechnologiesByHistoryIdQuery, IEnumerable<TechnologyResponse>> handler,
        HttpContext context)
    {
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var query = new GetTechnologiesByHistoryIdQuery(id, userId);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> ExportSinglePdf(
        int id,
        IQueryHandler<ExportSingleHistoryPdfQuery, PdfExportResponse> handler,
        HttpContext context)
    {
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
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
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var username = context.User.Identity?.Name ?? "Anonymous";

        var query = new ExportHistoryPdfQuery(userId, username);
        var result = await handler.Handle(query);

        return Results.File(
            result.PdfData,
            contentType: result.ContentType,
            fileDownloadName: result.FileName);
    }

    private static async Task<IResult> DeleteScanHistory(
        int id,
        ICommandHandler<DeleteScanHistoryCommand, DeleteScanHistoryResponse> handler,
        HttpContext context)
    {
        var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var command = new DeleteScanHistoryCommand(id, userId);
        await handler.Handle(command);

        return Results.Ok(new { message = "Scan history deleted successfully", historyId = id });
    }
}
