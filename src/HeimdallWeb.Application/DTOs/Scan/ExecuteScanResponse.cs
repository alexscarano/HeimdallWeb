namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for scan execution.
/// </summary>
public record ExecuteScanResponse(
    int HistoryId,
    string Target,
    string Summary,
    TimeSpan Duration,
    bool HasCompleted,
    DateTime CreatedDate
);
