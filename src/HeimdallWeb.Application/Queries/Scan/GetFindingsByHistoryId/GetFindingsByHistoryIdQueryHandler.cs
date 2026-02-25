using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Scan.GetFindingsByHistoryId;

/// <summary>
/// Handler for GetFindingsByHistoryIdQuery.
/// Retrieves all security findings for a scan, ordered by severity DESC (Critical → High → Medium → Low).
/// Validates ownership before returning data.
///
/// Source: HeimdallWebOld/Controllers/HistoryController.cs lines 91-106 (GetFindings method)
/// </summary>
public class GetFindingsByHistoryIdQueryHandler : IQueryHandler<GetFindingsByHistoryIdQuery, IEnumerable<FindingResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFindingsByHistoryIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<FindingResponse>> Handle(GetFindingsByHistoryIdQuery query, CancellationToken cancellationToken = default)
    {
        // Verify scan history exists by PublicId
        var scanHistory = await _unitOfWork.ScanHistories.GetByPublicIdAsync(query.HistoryId, cancellationToken);

        if (scanHistory == null)
            throw new NotFoundException("Scan history", query.HistoryId);

        // Verify ownership (users can only view their own findings, admins can view any)
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.RequestingUserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", query.RequestingUserId);

        if (user.UserType != UserType.Admin && scanHistory.UserId != user.UserId)
            throw new NotFoundException("Scan history", query.HistoryId); // Security: 404 instead of 403

        // Get findings using internal HistoryId
        var findings = await _unitOfWork.Findings.GetByHistoryIdAsync(scanHistory.HistoryId, cancellationToken);

        // Order by severity DESC (Critical → High → Medium → Low → Informational)
        var orderedFindings = findings
            .OrderByDescending(f => f.Severity)
            .ToList();

        // Map to response DTOs
        return orderedFindings.Select(f => new FindingResponse(
            FindingId: f.FindingId,
            Type: f.Type,
            Description: f.Description,
            Severity: f.Severity.ToString(),
            Evidence: f.Evidence,
            Recommendation: f.Recommendation,
            HistoryId: f.HistoryId,
            CreatedAt: f.CreatedAt
        ));
    }
}
