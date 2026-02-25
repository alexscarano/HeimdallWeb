using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Commands.Monitor.DeleteMonitor;

/// <summary>
/// Handles <see cref="DeleteMonitorCommand"/>.
/// Verifies that the target exists and belongs to the requesting user before deleting it.
/// </summary>
public class DeleteMonitorCommandHandler : ICommandHandler<DeleteMonitorCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMonitorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> Handle(
        DeleteMonitorCommand command,
        CancellationToken cancellationToken = default)
    {
        // Resolve user's internal integer ID from their public UUID
        var user = await _unitOfWork.Users.GetByPublicIdAsync(command.UserPublicId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", command.UserPublicId);

        var target = await _unitOfWork.MonitoredTargets.GetByIdAsync(
            command.MonitoredTargetId, cancellationToken);

        if (target == null)
            throw new NotFoundException("MonitoredTarget", command.MonitoredTargetId);

        // Enforce ownership — users can only delete their own monitors
        if (target.UserId != user.UserId)
            throw new ForbiddenException("You do not have permission to delete this monitored target.");

        await _unitOfWork.MonitoredTargets.DeleteAsync(target, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
