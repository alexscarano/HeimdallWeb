using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Notifications.Commands;

public class ClearAllNotificationsCommandHandler : ICommandHandler<ClearAllNotificationsCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ClearAllNotificationsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> Handle(ClearAllNotificationsCommand command, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Notifications.DeleteAllAsync(command.UserId, cancellationToken);
        // ExecuteDeleteAsync processes immediately on the DB side, 
        // depending on the repo implementation. We can still call SaveChangesAsync if needed,
        // but it's typically fine for ExecuteDeleteAsync.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
