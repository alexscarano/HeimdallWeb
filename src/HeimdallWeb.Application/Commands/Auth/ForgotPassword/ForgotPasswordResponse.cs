namespace HeimdallWeb.Application.Commands.Auth.ForgotPassword;

/// <summary>
/// Response DTO for the forgot-password endpoint.
/// Always returns the same neutral message to prevent email enumeration.
/// </summary>
public record ForgotPasswordResponse(string Message);
