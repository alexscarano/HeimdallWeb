using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Notifications.Commands;

/// <summary>
/// Handles <see cref="MarkNotificationReadCommand"/>.
/// Fetches the notification, verifies ownership, marks it as read, and persists the change.
/// Returns false if the notification does not exist or does not belong to the requesting user.
/// </summary>
public class MarkNotificationReadCommandHandler : ICommandHandler<MarkNotificationReadCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> Handle(
        MarkNotificationReadCommand command,
        CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(
            command.NotificationId,
            cancellationToken);

        if (notification == null)
            return false;

        // Enforce ownership — users can only mark their own notifications as read
        if (notification.UserId != command.UserId)
            return false;

        notification.MarkAsRead();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
