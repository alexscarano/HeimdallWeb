namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for DeleteScanHistoryCommand.
/// Confirms successful deletion with history details.
/// </summary>
public record DeleteScanHistoryResponse(
    bool Success,
    Guid HistoryId,
    string Target
);
