using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Notifications.Commands;

/// <summary>
/// Handles <see cref="MarkAllReadCommand"/>.
/// Marks all unread notifications for the requesting user as read via a bulk repository operation,
/// then persists the changes.
/// </summary>
public class MarkAllReadCommandHandler : ICommandHandler<MarkAllReadCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> Handle(
        MarkAllReadCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(command.UserId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
