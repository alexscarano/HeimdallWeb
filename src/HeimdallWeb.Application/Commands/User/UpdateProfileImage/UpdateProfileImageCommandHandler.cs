using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Commands.User.UpdateProfileImage;

/// <summary>
/// Handles profile image updates with validation and file management.
/// Validates image format (JPG, PNG, WEBP), size (2MB max), and ownership.
/// Manages file I/O: saves new image, deletes old image.
/// </summary>
public class UpdateProfileImageCommandHandler : ICommandHandler<UpdateProfileImageCommand, UpdateProfileImageResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private const int MaxImageSizeBytes = 2 * 1024 * 1024; // 2MB
    private const string UploadDirectory = "wwwroot/uploads/profiles";

    // Magic bytes for supported image formats
    private static readonly Dictionary<string, byte[]> ImageSignatures = new()
    {
        { "jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { "png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        { "webp", new byte[] { 0x52, 0x49, 0x46, 0x46 } } // RIFF header for WEBP
    };

    public UpdateProfileImageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateProfileImageResponse> Handle(UpdateProfileImageCommand request, CancellationToken ct = default)
    {
        // Validate input
        var validator = new UpdateProfileImageCommandValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            throw new ValidationException(errors);
        }

        // SECURITY: Verify ownership
        if (request.UserId != request.RequestingUserId)
        {
            throw new ForbiddenException("You can only update your own profile image");
        }

        // Get user from database
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Decode Base64 to byte array
        byte[] imageBytes;
        try
        {
            var cleanBase64 = request.ImageBase64;
            // Remove data URL prefix if present (data:image/png;base64,)
            if (request.ImageBase64.Contains(","))
            {
                cleanBase64 = request.ImageBase64.Split(',')[1];
            }

            imageBytes = Convert.FromBase64String(cleanBase64);
        }
        catch (Exception ex)
        {
            throw new ValidationException("ImageBase64", $"Invalid Base64 image data: {ex.Message}");
        }

        // Validate image size
        if (imageBytes.Length > MaxImageSizeBytes)
        {
            throw new ValidationException("ImageBase64", $"Image size ({imageBytes.Length / 1024 / 1024}MB) exceeds maximum allowed size (2MB)");
        }

        // Validate image format using magic bytes
        if (!IsValidImageFormat(imageBytes))
        {
            throw new ValidationException("ImageBase64", "Invalid image format. Only JPG, PNG, and WEBP are supported.");
        }

        // Generate unique filename
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var filename = $"{request.UserId}_{timestamp}.jpg";
        var relativePath = $"uploads/profiles/{filename}";
        var fullPath = Path.Combine(UploadDirectory, filename);

        // Create directory if not exists
        Directory.CreateDirectory(UploadDirectory);

        // Save new image to disk
        await File.WriteAllBytesAsync(fullPath, imageBytes, ct);

        // Delete old profile image if exists
        if (!string.IsNullOrWhiteSpace(user.ProfileImage))
        {
            var oldImagePath = Path.Combine("wwwroot", user.ProfileImage.TrimStart('/'));
            if (File.Exists(oldImagePath))
            {
                try
                {
                    File.Delete(oldImagePath);
                }
                catch
                {
                    // Ignore deletion errors for old image
                    // Log could be added here if needed
                }
            }
        }

        // Update user profile image using domain method
        user.UpdateProfileImage(relativePath);

        await _unitOfWork.SaveChangesAsync(ct);

        // Log profile image update
        await LogProfileImageUpdateAsync(user.UserId, relativePath, ct);

        return new UpdateProfileImageResponse(
            UserId: user.UserId,
            ProfileImagePath: relativePath
        );
    }

    private bool IsValidImageFormat(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length < 8)
            return false;

        foreach (var signature in ImageSignatures.Values)
        {
            if (imageBytes.Take(signature.Length).SequenceEqual(signature))
                return true;
        }

        return false;
    }

    private async Task LogProfileImageUpdateAsync(int userId, string imagePath, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.PROFILE_IMAGE_UPDATED,
            level: "Info",
            message: "Profile image updated",
            source: "UpdateProfileImageCommandHandler",
            details: $"New image path: {imagePath}",
            userId: userId
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
