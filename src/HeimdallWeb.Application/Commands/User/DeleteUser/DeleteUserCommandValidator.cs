using FluentValidation;

namespace HeimdallWeb.Application.Commands.User.DeleteUser;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.RequestingUserId)
            .GreaterThan(0).WithMessage("Requesting User ID must be greater than 0");

        // Security check: user can only delete themselves
        RuleFor(x => x)
            .Must(cmd => cmd.UserId == cmd.RequestingUserId)
            .WithMessage("You can only delete your own account")
            .WithName("Authorization");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required for account deletion")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.ConfirmDelete)
            .Must(x => x == true).WithMessage("Delete confirmation is required")
            .WithName("ConfirmDelete");
    }
}
