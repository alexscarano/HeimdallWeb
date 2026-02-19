namespace HeimdallWeb.Application.Commands.Auth.ForgotPassword;

/// <summary>
/// Command to initiate the forgot-password flow.
/// Always returns a generic success response to prevent email enumeration attacks.
/// </summary>
public record ForgotPasswordCommand(
    string Email,
    string RemoteIp
);
