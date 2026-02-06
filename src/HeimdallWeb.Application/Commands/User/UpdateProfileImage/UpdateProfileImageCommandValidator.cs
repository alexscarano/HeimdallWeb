using FluentValidation;

namespace HeimdallWeb.Application.Commands.User.UpdateProfileImage;

public class UpdateProfileImageCommandValidator : AbstractValidator<UpdateProfileImageCommand>
{
    private const int MaxImageSizeBytes = 2 * 1024 * 1024; // 2MB

    public UpdateProfileImageCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.RequestingUserId)
            .GreaterThan(0).WithMessage("Requesting user ID must be greater than 0");

        RuleFor(x => x)
            .Must(x => x.UserId == x.RequestingUserId)
            .WithMessage("You can only update your own profile image");

        RuleFor(x => x.ImageBase64)
            .NotEmpty().WithMessage("Image data is required")
            .Must(BeValidBase64).WithMessage("Image must be valid Base64 encoded data")
            .Must(BeValidImageSize).WithMessage("Image size must not exceed 2MB");
    }

    private bool BeValidBase64(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return false;

        try
        {
            // Remove data URL prefix if present (data:image/png;base64,)
            var cleanBase64 = base64String;
            if (base64String.Contains(","))
            {
                cleanBase64 = base64String.Split(',')[1];
            }

            Convert.FromBase64String(cleanBase64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool BeValidImageSize(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return false;

        try
        {
            // Remove data URL prefix if present
            var cleanBase64 = base64String;
            if (base64String.Contains(","))
            {
                cleanBase64 = base64String.Split(',')[1];
            }

            var imageBytes = Convert.FromBase64String(cleanBase64);
            return imageBytes.Length <= MaxImageSizeBytes;
        }
        catch
        {
            return false;
        }
    }
}
