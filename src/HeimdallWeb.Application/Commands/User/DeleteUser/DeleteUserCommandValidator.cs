using FluentValidation;

namespace HeimdallWeb.Application.Commands.User.DeleteUser;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting User ID is required");

        // Password is optional for admins deleting other users
        // But required when user deletes their own account
        RuleFor(x => x.Password)
            .NotEmpty()
            .When(x => x.UserId == x.RequestingUserId)
            .WithMessage("Password is required when deleting your own account");

        RuleFor(x => x.ConfirmDelete)
            .Must(x => x == true).WithMessage("Delete confirmation is required")
            .WithName("ConfirmDelete");
    }
}
