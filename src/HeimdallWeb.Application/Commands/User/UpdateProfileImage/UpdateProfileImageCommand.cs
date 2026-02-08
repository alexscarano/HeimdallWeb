namespace HeimdallWeb.Application.Commands.User.UpdateProfileImage;

/// <summary>
/// Command to update a user's profile image.
/// Validates image format, size (max 2MB), and ownership.
/// Security: Users can only update their own profile image.
/// </summary>
public record UpdateProfileImageCommand(
    Guid UserId,
    string ImageBase64,
    Guid RequestingUserId
);
