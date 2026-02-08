namespace HeimdallWeb.Application.DTOs.Admin;

/// <summary>
/// Response DTO for DeleteUserByAdminCommand.
/// Confirms successful user deletion by administrator.
/// </summary>
public record DeleteUserByAdminResponse(
    bool Success,
    Guid DeletedUserId,
    string DeletedUsername
);
