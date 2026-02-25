namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for scan execution.
/// ProfileId is echoed back when the caller supplied one in the request.
/// IsCached indicates whether the result was served from the scan cache rather than a live scan.
/// </summary>
public record ExecuteScanResponse(
    Guid HistoryId,
    string Target,
    string Summary,
    TimeSpan Duration,
    bool HasCompleted,
    DateTime CreatedDate,
    int? Score,
    string? Grade,
    int? ProfileId = null,
    bool IsCached = false
);
