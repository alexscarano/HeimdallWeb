namespace HeimdallWeb.Application.DTOs.User;

/// <summary>
/// Response DTO for DeleteUserCommand.
/// Contains confirmation message and deleted user ID.
/// </summary>
public record DeleteUserResponse(
    string Message,
    int UserId
);
