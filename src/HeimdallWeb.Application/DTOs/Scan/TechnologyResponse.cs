namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for detected technologies.
/// Represents a technology/framework detected during scanning.
/// </summary>
public record TechnologyResponse(
    int TechnologyId,
    string Name,
    string? Version,
    string Category,
    string? Description,
    int? HistoryId,
    DateTime CreatedAt
);
