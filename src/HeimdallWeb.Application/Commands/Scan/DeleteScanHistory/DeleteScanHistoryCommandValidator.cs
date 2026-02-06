using FluentValidation;

namespace HeimdallWeb.Application.Commands.Scan.DeleteScanHistory;

public class DeleteScanHistoryCommandValidator : AbstractValidator<DeleteScanHistoryCommand>
{
    public DeleteScanHistoryCommandValidator()
    {
        RuleFor(x => x.HistoryId)
            .GreaterThan(0).WithMessage("History ID must be greater than 0");

        RuleFor(x => x.RequestingUserId)
            .GreaterThan(0).WithMessage("Requesting user ID must be greater than 0");
    }
}
