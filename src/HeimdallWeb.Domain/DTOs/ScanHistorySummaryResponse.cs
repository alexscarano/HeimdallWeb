namespace HeimdallWeb.Domain.DTOs;

/// <summary>
/// Summary response for a single scan history in a list.
/// Lighter version without full JSON data.
/// </summary>
public record ScanHistorySummaryResponse(
    Guid HistoryId,
    string Target,
    DateTime CreatedDate,
    string? Duration,
    bool HasCompleted,
    string? Summary,
    int FindingsCount,
    int TechnologiesCount
);
