namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for detailed scan history information.
/// Includes all related entities: findings, technologies, and AI summary.
/// </summary>
public record ScanHistoryDetailResponse(
    int HistoryId,
    string Target,
    string? RawJsonResult,
    DateTime CreatedDate,
    int UserId,
    string? Duration,  // String format (e.g., "00:02:15")
    bool HasCompleted,
    string? Summary,
    IEnumerable<FindingResponse> Findings,
    IEnumerable<TechnologyResponse> Technologies,
    IASummaryResponse? IASummary
);
