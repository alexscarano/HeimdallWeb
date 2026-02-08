namespace HeimdallWeb.Application.DTOs.Auth;

/// <summary>
/// Request DTO for user registration.
/// </summary>
public record RegisterUserRequest(
    string Email,
    string Username,
    string Password
);
