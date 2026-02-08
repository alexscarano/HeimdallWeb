namespace HeimdallWeb.Application.DTOs.Auth;

/// <summary>
/// Response DTO for successful login.
/// </summary>
public record LoginResponse(
    Guid UserId,
    string Username,
    string Email,
    int UserType,
    string Token,
    bool IsActive
);
