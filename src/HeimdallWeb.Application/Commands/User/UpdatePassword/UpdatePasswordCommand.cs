namespace HeimdallWeb.Application.Commands.User.UpdatePassword;

/// <summary>
/// Command to update user password.
/// Requires current password for security verification.
/// </summary>
public record UpdatePasswordCommand(
    Guid UserId,
    Guid RequestingUserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
