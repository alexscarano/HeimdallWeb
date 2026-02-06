namespace HeimdallWeb.Application.DTOs.Auth;

/// <summary>
/// Request DTO for user login.
/// Adapted from LoginDTO in HeimdallWebOld.
/// </summary>
public record LoginRequest(
    string EmailOrLogin,
    string Password
);
