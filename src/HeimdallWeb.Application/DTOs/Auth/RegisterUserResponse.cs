namespace HeimdallWeb.Application.DTOs.Auth;

/// <summary>
/// Response DTO for successful user registration.
/// </summary>
public record RegisterUserResponse(
    int UserId,
    string Username,
    string Email,
    int UserType,
    string Token,
    bool IsActive
);
