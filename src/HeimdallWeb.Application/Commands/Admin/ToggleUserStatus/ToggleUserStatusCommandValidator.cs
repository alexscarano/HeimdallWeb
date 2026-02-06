using FluentValidation;
using HeimdallWeb.Domain.Enums;

namespace HeimdallWeb.Application.Commands.Admin.ToggleUserStatus;

public class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.RequestingUserType)
            .Equal(UserType.Admin).WithMessage("Only administrators can toggle user status");
    }
}
