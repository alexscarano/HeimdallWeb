namespace HeimdallWeb.Application.DTOs.Admin;

/// <summary>
/// Response DTO for ToggleUserStatusCommand.
/// Returns updated user status information.
/// </summary>
public record ToggleUserStatusResponse(
    Guid UserId,
    string Username,
    bool IsActive
);
