namespace HeimdallWeb.Application.DTOs.History;

/// <summary>
/// Response DTO for scan history.
/// </summary>
public record HistoryResponse(
    Guid HistoryId,
    string Target,
    string? Summary,
    TimeSpan Duration,
    bool HasCompleted,
    DateTime CreatedDate,
    Guid UserId
);
