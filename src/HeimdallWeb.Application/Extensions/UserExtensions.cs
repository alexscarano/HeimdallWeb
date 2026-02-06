using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Application.Extensions;

/// <summary>
/// Extension methods for mapping User entity to DTOs.
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Maps User entity to UserProfileResponse DTO.
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>UserProfileResponse DTO</returns>
    public static UserProfileResponse ToProfileDto(this User user)
    {
        return new UserProfileResponse(
            UserId: user.UserId,
            Username: user.Username,
            Email: user.Email.Value, // EmailAddress is a Value Object
            UserType: (int)user.UserType,
            IsActive: user.IsActive,
            ProfileImage: user.ProfileImage,
            CreatedAt: user.CreatedAt
        );
    }
}
