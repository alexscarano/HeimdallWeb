namespace HeimdallWeb.Application.DTOs.Auth;

/// <summary>
/// Response DTO for successful user registration.
/// </summary>
public record RegisterUserResponse(
    Guid UserId,
    string Username,
    string Email,
    int UserType,
    string Token,
    bool IsActive
);
