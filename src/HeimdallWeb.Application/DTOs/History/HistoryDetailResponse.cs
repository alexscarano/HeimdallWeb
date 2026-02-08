namespace HeimdallWeb.Application.DTOs.History;

/// <summary>
/// Response DTO for detailed scan history with findings and technologies.
/// </summary>
public record HistoryDetailResponse(
    Guid HistoryId,
    string Target,
    string? Summary,
    string? RawJsonResult,
    TimeSpan Duration,
    bool HasCompleted,
    DateTime CreatedDate,
    Guid UserId,
    IReadOnlyList<FindingResponse> Findings,
    IReadOnlyList<TechnologyResponse> Technologies
);
