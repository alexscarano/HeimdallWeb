namespace HeimdallWeb.Application.Commands.User.UpdateUser;

/// <summary>
/// Command to update user profile information (username and/or email).
/// Users can only update their own profiles for security.
/// </summary>
public record UpdateUserCommand(
    Guid UserId,
    Guid RequestingUserId,
    string? NewUsername,
    string? NewEmail
);
