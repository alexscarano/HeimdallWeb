namespace HeimdallWeb.Application.DTOs.User;

/// <summary>
/// Response DTO for UpdateUserCommand.
/// Contains updated user profile information.
/// </summary>
public record UpdateUserResponse(
    int UserId,
    string Username,
    string Email,
    int UserType,
    bool IsActive
);
