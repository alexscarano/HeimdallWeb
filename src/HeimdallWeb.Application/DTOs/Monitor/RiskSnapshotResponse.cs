namespace HeimdallWeb.Application.DTOs.Monitor;

/// <summary>
/// Response DTO representing a point-in-time security snapshot for a monitored target.
/// </summary>
public record RiskSnapshotResponse(
    int Id,
    int Score,
    string Grade,
    int FindingsCount,
    int CriticalCount,
    int HighCount,
    DateTime CreatedAt
);
