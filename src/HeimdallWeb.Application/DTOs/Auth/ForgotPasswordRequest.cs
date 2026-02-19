namespace HeimdallWeb.Application.DTOs.Auth;

/// <summary>
/// Request DTO for the forgot-password endpoint.
/// </summary>
public record ForgotPasswordRequest(string Email);
