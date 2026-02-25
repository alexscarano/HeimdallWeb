using FluentValidation;
using System.Text.RegularExpressions;

namespace HeimdallWeb.Application.Commands.User.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting User ID is required");

        // Security check: user can only update themselves
        RuleFor(x => x)
            .Must(cmd => cmd.UserId == cmd.RequestingUserId)
            .WithMessage("You can only update your own profile")
            .WithName("Authorization");

        // At least one field must be provided
        RuleFor(x => x)
            .Must(cmd => !string.IsNullOrWhiteSpace(cmd.NewUsername) || !string.IsNullOrWhiteSpace(cmd.NewEmail))
            .WithMessage("At least one field (username or email) must be provided")
            .WithName("UpdateFields");

        // Username validation (optional, but if provided must be valid)
        When(x => !string.IsNullOrWhiteSpace(x.NewUsername), () =>
        {
            RuleFor(x => x.NewUsername)
                .Length(6, 30).WithMessage("Username must be between 6 and 30 characters")
                .Must(username => Regex.IsMatch(username!, @"^[a-zA-Z0-9_-]+$"))
                .WithMessage("Username can only contain letters, numbers, hyphens, and underscores");
        });

        // Email validation (optional, but if provided must be valid)
        When(x => !string.IsNullOrWhiteSpace(x.NewEmail), () =>
        {
            RuleFor(x => x.NewEmail)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
        });
    }
}
