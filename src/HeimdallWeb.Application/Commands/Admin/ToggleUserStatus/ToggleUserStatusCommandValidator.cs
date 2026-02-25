using FluentValidation;
using HeimdallWeb.Domain.Enums;

namespace HeimdallWeb.Application.Commands.Admin.ToggleUserStatus;

public class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RequestingUserType)
            .Equal(UserType.Admin).WithMessage("Only administrators can toggle user status");
    }
}
