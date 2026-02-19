using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Monitor;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Monitor.GetMonitorHistory;

/// <summary>
/// Handles <see cref="GetMonitorHistoryQuery"/>.
/// Verifies ownership then returns the most recent 30 risk snapshots for the target.
/// </summary>
public class GetMonitorHistoryQueryHandler : IQueryHandler<GetMonitorHistoryQuery, IEnumerable<RiskSnapshotResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMonitorHistoryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<RiskSnapshotResponse>> Handle(
        GetMonitorHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        // Resolve user's internal integer ID from their public UUID
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.UserPublicId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", query.UserPublicId);

        // Verify the target exists and belongs to the requesting user
        var target = await _unitOfWork.MonitoredTargets.GetByIdAsync(query.MonitoredTargetId, cancellationToken);

        if (target == null)
            throw new NotFoundException("MonitoredTarget", query.MonitoredTargetId);

        if (target.UserId != user.UserId)
            throw new ForbiddenException("You do not have permission to view this monitored target's history.");

        var snapshots = await _unitOfWork.RiskSnapshots.GetByTargetIdAsync(
            query.MonitoredTargetId, limit: 30, ct: cancellationToken);

        return snapshots.Select(s => new RiskSnapshotResponse(
            Id: s.Id,
            Score: s.Score,
            Grade: s.Grade,
            FindingsCount: s.FindingsCount,
            CriticalCount: s.CriticalCount,
            HighCount: s.HighCount,
            CreatedAt: s.CreatedAt));
    }
}
