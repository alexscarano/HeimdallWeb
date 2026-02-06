namespace HeimdallWeb.Application.DTOs.Admin;

/// <summary>
/// Response DTO for ToggleUserStatusCommand.
/// Returns updated user status information.
/// </summary>
public record ToggleUserStatusResponse(
    int UserId,
    string Username,
    bool IsActive
);
