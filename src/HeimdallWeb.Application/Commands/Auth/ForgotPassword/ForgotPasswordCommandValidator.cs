using FluentValidation;

namespace HeimdallWeb.Application.Commands.Auth.ForgotPassword;

/// <summary>
/// Validator for ForgotPasswordCommand.
/// Validates email format before processing to prevent invalid input.
/// Note: Email enumeration prevention is handled in the handler, not here.
/// </summary>
public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email address is not valid.");

        RuleFor(x => x.RemoteIp)
            .NotEmpty().WithMessage("Remote IP is required.");
    }
}
