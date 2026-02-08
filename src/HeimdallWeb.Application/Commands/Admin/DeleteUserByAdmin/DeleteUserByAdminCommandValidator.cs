using FluentValidation;
using HeimdallWeb.Domain.Enums;

namespace HeimdallWeb.Application.Commands.Admin.DeleteUserByAdmin;

public class DeleteUserByAdminCommandValidator : AbstractValidator<DeleteUserByAdminCommand>
{
    public DeleteUserByAdminCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        // NOTE: Authorization check (UserType == Admin) is performed in the handler
        // to return 403 Forbidden instead of 400 Bad Request

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user ID is required");

        RuleFor(x => x)
            .Must(x => x.UserId != x.RequestingUserId)
            .WithMessage("Cannot delete yourself");
    }
}
