namespace HeimdallWeb.Application.Queries.Scan.GetScanHistoryById;

/// <summary>
/// Query to retrieve a scan history by its ID with full details.
/// Includes findings, technologies, and AI summary.
/// Validates ownership before returning data.
/// </summary>
/// <param name="HistoryId">The scan history ID</param>
/// <param name="RequestingUserId">The user requesting the data (for ownership verification)</param>
public record GetScanHistoryByIdQuery(
    Guid HistoryId,
    Guid RequestingUserId
);
