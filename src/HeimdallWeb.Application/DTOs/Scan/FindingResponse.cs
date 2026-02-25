namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for security findings.
/// Represents a vulnerability or security issue found during scanning.
/// </summary>
public record FindingResponse(
    int FindingId,
    string Type,
    string Description,
    string Severity,
    string? Evidence,
    string? Recommendation,
    int? HistoryId,
    DateTime CreatedAt
);
