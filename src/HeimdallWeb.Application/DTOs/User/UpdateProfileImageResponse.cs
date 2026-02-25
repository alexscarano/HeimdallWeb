namespace HeimdallWeb.Application.DTOs.User;

/// <summary>
/// Response DTO for UpdateProfileImageCommand.
/// Returns updated profile image path.
/// </summary>
public record UpdateProfileImageResponse(
    Guid UserId,
    string ProfileImagePath
);
