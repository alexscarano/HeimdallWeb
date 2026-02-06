namespace HeimdallWeb.Application.DTOs.History;

/// <summary>
/// Response DTO for detected technology.
/// Adapted from TechnologyDTO in HeimdallWebOld.
/// </summary>
public record TechnologyResponse(
    int TechnologyId,
    string Name,
    string? Version,
    string Category,
    string Description
);
