namespace HeimdallWeb.Application.DTOs.Auth;

/// <summary>
/// Request DTO for the reset-password endpoint.
/// </summary>
public record ResetPasswordRequest(string Token, string NewPassword);
