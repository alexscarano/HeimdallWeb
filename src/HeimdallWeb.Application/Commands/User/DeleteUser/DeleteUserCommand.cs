namespace HeimdallWeb.Application.Commands.User.DeleteUser;

/// <summary>
/// Command to delete a user account.
/// Requires password confirmation and explicit delete confirmation for security.
/// Users can only delete their own accounts.
/// </summary>
public record DeleteUserCommand(
    Guid UserId,
    Guid RequestingUserId,
    string Password,
    bool ConfirmDelete
);
