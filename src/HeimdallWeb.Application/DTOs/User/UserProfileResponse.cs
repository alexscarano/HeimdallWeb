namespace HeimdallWeb.Application.DTOs.User;

/// <summary>
/// Response DTO for user profile details.
/// Used in GetUserProfileQuery.
/// </summary>
/// <param name="UserId">User unique identifier</param>
/// <param name="Username">Username (6-30 characters)</param>
/// <param name="Email">Email address</param>
/// <param name="UserType">User type (1=Regular, 2=Admin)</param>
/// <param name="IsActive">Whether the user account is active</param>
/// <param name="ProfileImage">Profile image URL (optional)</param>
/// <param name="CreatedAt">Account creation timestamp</param>
public record UserProfileResponse(
    int UserId,
    string Username,
    string Email,
    int UserType,
    bool IsActive,
    string? ProfileImage,
    DateTime CreatedAt
);
