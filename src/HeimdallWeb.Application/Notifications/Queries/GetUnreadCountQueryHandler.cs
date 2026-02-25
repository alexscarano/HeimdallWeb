using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Notifications.Queries;

/// <summary>
/// Handles <see cref="GetUnreadCountQuery"/>.
/// Returns the total count of unread notifications for the requesting user.
/// </summary>
public class GetUnreadCountQueryHandler : IQueryHandler<GetUnreadCountQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnreadCountQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<int> Handle(
        GetUnreadCountQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(query.UserId, cancellationToken);
    }
}
