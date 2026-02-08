using FluentValidation;
using HeimdallWeb.Domain.Enums;

namespace HeimdallWeb.Application.Commands.Admin.DeleteUserByAdmin;

public class DeleteUserByAdminCommandValidator : AbstractValidator<DeleteUserByAdminCommand>
{
    public DeleteUserByAdminCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        // NOTE: Authorization check (UserType == Admin) is performed in the handler
        // to return 403 Forbidden instead of 400 Bad Request

        RuleFor(x => x.RequestingUserId)
            .GreaterThan(0).WithMessage("Requesting user ID must be greater than 0");

        RuleFor(x => x)
            .Must(x => x.UserId != x.RequestingUserId)
            .WithMessage("Cannot delete yourself");
    }
}
