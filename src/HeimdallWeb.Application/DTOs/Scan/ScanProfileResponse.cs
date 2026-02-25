namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for a scan profile returned by the API.
/// </summary>
public record ScanProfileResponse(
    int Id,
    string Name,
    string Description,
    string ConfigJson,
    bool IsSystem
);
