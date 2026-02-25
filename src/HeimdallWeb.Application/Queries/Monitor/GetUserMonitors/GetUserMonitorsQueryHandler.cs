using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Monitor;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Monitor.GetUserMonitors;

/// <summary>
/// Handles <see cref="GetUserMonitorsQuery"/>.
/// Returns all monitored targets owned by the requesting user.
/// </summary>
public class GetUserMonitorsQueryHandler : IQueryHandler<GetUserMonitorsQuery, IEnumerable<MonitoredTargetResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserMonitorsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<MonitoredTargetResponse>> Handle(
        GetUserMonitorsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Resolve user's internal integer ID from their public UUID
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.UserPublicId, cancellationToken);
        if (user == null)
            return Enumerable.Empty<MonitoredTargetResponse>();

        var targets = await _unitOfWork.MonitoredTargets.GetByUserIdAsync(user.UserId, cancellationToken);

        return targets.Select(t => new MonitoredTargetResponse(
            Id: t.Id,
            Url: t.Url,
            Frequency: t.Frequency.ToString(),
            LastCheck: t.LastCheck,
            NextCheck: t.NextCheck,
            IsActive: t.IsActive,
            CreatedAt: t.CreatedAt));
    }
}
