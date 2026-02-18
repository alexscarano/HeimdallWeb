using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Application.Commands.User.UpdateProfileImage;

/// <summary>
/// Handles profile image updates with validation and file management.
/// Validates image format (JPG, PNG, WEBP), size (2MB max), and ownership.
/// Manages file I/O: saves new image, deletes old image.
/// </summary>
public class UpdateProfileImageCommandHandler : ICommandHandler<UpdateProfileImageCommand, UpdateProfileImageResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Microsoft.Extensions.Logging.ILogger<UpdateProfileImageCommandHandler> _logger;
    private const int MaxImageSizeBytes = 2 * 1024 * 1024; // 2MB
    private const string UploadDirectory = "wwwroot/uploads/profiles";

    // Magic bytes for supported image formats
    private static readonly Dictionary<string, byte[]> ImageSignatures = new()
    {
        { "jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { "png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        { "webp", new byte[] { 0x52, 0x49, 0x46, 0x46 } } // RIFF header for WEBP
    };

    public UpdateProfileImageCommandHandler(IUnitOfWork unitOfWork, Microsoft.Extensions.Logging.ILogger<UpdateProfileImageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateProfileImageResponse> Handle(UpdateProfileImageCommand request, CancellationToken ct = default)
    {
        _logger.LogInformation("UpdateProfileImage started for User {UserId}", request.UserId);

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

        // Get user from database WITH TRACKING by PublicId (so EF Core can save changes)
        var user = await _unitOfWork.Users.GetByPublicIdForUpdateAsync(request.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
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
            _logger.LogInformation("Image bytes decoded. Length: {Length}", imageBytes.Length);
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
            _logger.LogWarning("Invalid image format detected.");
            throw new ValidationException("ImageBase64", "Invalid image format. Only JPG, PNG, and WEBP are supported.");
        }

        // Generate unique filename
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var filename = $"{request.UserId}_{timestamp}.jpg";
        
        // Use absolute path resolution for Docker/Linux compatibility
        // This ensures we're writing to the actual running application's wwwroot
        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadDir = Path.Combine(webRootPath, "uploads", "profiles");
        var fullPath = Path.Combine(uploadDir, filename);
        
        _logger.LogInformation("Resolved webRootPath: {WebRootPath}", webRootPath);
        _logger.LogInformation("Resolved uploadDir: {UploadDir}", uploadDir);
        _logger.LogInformation("Resolved fullPath to save: {FullPath}", fullPath);

        // Store relative path for database (browser access)
        var relativePath = $"uploads/profiles/{filename}";

        // Create directory if not exists
        if (!Directory.Exists(uploadDir))
        {
            _logger.LogInformation("Directory does not exist. Creating: {UploadDir}", uploadDir);
            Directory.CreateDirectory(uploadDir);
        }

        // Save new image to disk
        await File.WriteAllBytesAsync(fullPath, imageBytes, ct);
        _logger.LogInformation("File written successfully.");

        // Delete old profile image if exists
        if (!string.IsNullOrWhiteSpace(user.ProfileImage))
        {
            var oldImageFileName = Path.GetFileName(user.ProfileImage);
            var oldImagePath = Path.Combine(uploadDir, oldImageFileName);
            
            _logger.LogInformation("Attempting to delete old image at: {OldImagePath}", oldImagePath);
            
            if (File.Exists(oldImagePath))
            {
                try
                {
                    File.Delete(oldImagePath);
                    _logger.LogInformation("Old image deleted.");
                }
                catch (Exception ex)
                {
                    // Ignore deletion errors for old image
                    // Log could be added here if needed
                    _logger.LogError(ex, "Failed to delete old image.");
                }
            }
            else 
            {
                 _logger.LogWarning("Old image file not found at calculated path.");
            }
        }

        // Update user profile image using domain method
        user.UpdateProfileImage(relativePath);

        // Log profile image update (create log entity before saving)
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.PROFILE_IMAGE_UPDATED,
            level: "Info",
            message: "Profile image updated",
            source: "UpdateProfileImageCommandHandler",
            details: $"New image path: {relativePath}",
            userId: user.UserId
        );
        await _unitOfWork.AuditLogs.AddAsync(log, ct);

        // Single SaveChanges for both user update and audit log
        await _unitOfWork.SaveChangesAsync(ct);

        return new UpdateProfileImageResponse(
            UserId: user.PublicId,
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


}
