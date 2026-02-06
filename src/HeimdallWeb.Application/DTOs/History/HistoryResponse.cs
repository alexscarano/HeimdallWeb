namespace HeimdallWeb.Application.DTOs.History;

/// <summary>
/// Response DTO for scan history.
/// </summary>
public record HistoryResponse(
    int HistoryId,
    string Target,
    string? Summary,
    TimeSpan Duration,
    bool HasCompleted,
    DateTime CreatedDate,
    int UserId
);
