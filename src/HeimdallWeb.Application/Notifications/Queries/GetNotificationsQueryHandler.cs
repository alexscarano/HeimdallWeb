using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.Notifications.DTOs;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Notifications.Queries;

/// <summary>
/// Handles <see cref="GetNotificationsQuery"/>.
/// Returns a paginated list of notifications for the requesting user,
/// mapped to <see cref="NotificationResponse"/> DTOs.
/// </summary>
public class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, IEnumerable<NotificationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<NotificationResponse>> Handle(
        GetNotificationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(
            query.UserId,
            query.Page,
            query.PageSize,
            cancellationToken);

        return notifications.Select(n => new NotificationResponse(
            Id: n.Id,
            Title: n.Title,
            Body: n.Body,
            Type: n.Type.ToString(),
            IsRead: n.IsRead,
            CreatedAt: n.CreatedAt,
            ReadAt: n.ReadAt));
    }
}
