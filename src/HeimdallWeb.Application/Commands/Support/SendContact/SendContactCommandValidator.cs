using FluentValidation;

namespace HeimdallWeb.Application.Commands.Support.SendContact;

/// <summary>
/// FluentValidation validator for SendContactCommand.
/// </summary>
public class SendContactCommandValidator : AbstractValidator<SendContactCommand>
{
    public SendContactCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email address is not valid.")
            .MaximumLength(75).WithMessage("Email cannot exceed 75 characters.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MinimumLength(5).WithMessage("Subject must be at least 5 characters long.")
            .MaximumLength(150).WithMessage("Subject cannot exceed 150 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MinimumLength(10).WithMessage("Message must be at least 10 characters long.")
            .MaximumLength(2000).WithMessage("Message cannot exceed 2000 characters.");
    }
}
