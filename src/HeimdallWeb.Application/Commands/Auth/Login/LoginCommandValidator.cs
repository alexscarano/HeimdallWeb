using FluentValidation;

namespace HeimdallWeb.Application.Commands.Auth.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("Email or username is required")
            .MinimumLength(3).WithMessage("Email or username must be at least 3 characters")
            .MaximumLength(100).WithMessage("Email or username is too long");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password is too long");

        RuleFor(x => x.RemoteIp)
            .NotEmpty().WithMessage("Remote IP is required");
    }
}
