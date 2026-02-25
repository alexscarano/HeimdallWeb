namespace HeimdallWeb.Application.DTOs.Monitor;

/// <summary>
/// Response DTO representing a monitored target registered by a user.
/// </summary>
public record MonitoredTargetResponse(
    int Id,
    string Url,
    string Frequency,
    DateTime? LastCheck,
    DateTime NextCheck,
    bool IsActive,
    DateTime CreatedAt
);
