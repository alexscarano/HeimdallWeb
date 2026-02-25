using HeimdallWeb.Application.DTOs.Scan;

namespace HeimdallWeb.Application.Queries.Scan.GetAISummaryByHistoryId;

/// <summary>
/// Query to retrieve AI summary for a scan history.
/// Includes userId for ownership validation.
/// </summary>
public record GetAISummaryByHistoryIdQuery(
    Guid HistoryId,
    Guid RequestingUserId
);
