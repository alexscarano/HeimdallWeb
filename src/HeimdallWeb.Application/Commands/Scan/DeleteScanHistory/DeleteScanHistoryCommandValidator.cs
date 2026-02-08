using FluentValidation;

namespace HeimdallWeb.Application.Commands.Scan.DeleteScanHistory;

public class DeleteScanHistoryCommandValidator : AbstractValidator<DeleteScanHistoryCommand>
{
    public DeleteScanHistoryCommandValidator()
    {
        RuleFor(x => x.HistoryId)
            .NotEmpty().WithMessage("History ID is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user ID is required");
    }
}
