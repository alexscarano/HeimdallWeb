using FluentValidation;

namespace HeimdallWeb.Application.Commands.User.UpdatePassword;

public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting User ID is required");

        // Security: user can only update their own password
        RuleFor(x => x)
            .Must(cmd => cmd.UserId == cmd.RequestingUserId)
            .WithMessage("You can only update your own password")
            .WithName("Authorization");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");

        // New password must be different from current
        RuleFor(x => x)
            .Must(cmd => cmd.NewPassword != cmd.CurrentPassword)
            .WithMessage("New password must be different from current password")
            .WithName("NewPassword");
    }
}
