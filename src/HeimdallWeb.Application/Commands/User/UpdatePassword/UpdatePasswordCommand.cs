namespace HeimdallWeb.Application.Commands.User.UpdatePassword;

/// <summary>
/// Command to update user password.
/// Requires current password for security verification.
/// </summary>
public record UpdatePasswordCommand(
    int UserId,
    int RequestingUserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
