namespace HeimdallWeb.Application.Commands.Auth.ResetPassword;

/// <summary>
/// Command to complete the password reset flow using a valid, unexpired token.
/// </summary>
public record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string RemoteIp
);
