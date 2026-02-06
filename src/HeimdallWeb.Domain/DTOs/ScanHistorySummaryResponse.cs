namespace HeimdallWeb.Domain.DTOs;

/// <summary>
/// Summary response for a single scan history in a list.
/// Lighter version without full JSON data.
/// </summary>
public record ScanHistorySummaryResponse(
    int HistoryId,
    string Target,
    DateTime CreatedDate,
    string? Duration,  // String format (e.g., "00:02:15")
    bool HasCompleted,
    string? Summary,
    int FindingsCount,
    int TechnologiesCount
);
