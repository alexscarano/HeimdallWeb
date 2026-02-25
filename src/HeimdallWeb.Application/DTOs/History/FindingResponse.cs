namespace HeimdallWeb.Application.DTOs.History;

/// <summary>
/// Response DTO for security finding.
/// Adapted from FindingDTO in HeimdallWebOld.
/// </summary>
public record FindingResponse(
    int FindingId,
    string Description,
    string Category,
    string Severity,
    string Evidence,
    string Recommendation
);
