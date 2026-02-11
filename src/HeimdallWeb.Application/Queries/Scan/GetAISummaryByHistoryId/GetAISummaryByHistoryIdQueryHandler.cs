using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Scan.GetAISummaryByHistoryId;

/// <summary>
/// Handler for GetAISummaryByHistoryIdQuery.
/// Retrieves AI-generated summary for a scan, with ownership validation.
/// Returns null if no AI summary exists (expected for some scans).
/// </summary>
public class GetAISummaryByHistoryIdQueryHandler : IQueryHandler<GetAISummaryByHistoryIdQuery, IASummaryResponse?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAISummaryByHistoryIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IASummaryResponse?> Handle(GetAISummaryByHistoryIdQuery query, CancellationToken cancellationToken = default)
    {
        // Verify scan history exists by PublicId
        var scanHistory = await _unitOfWork.ScanHistories.GetByPublicIdAsync(query.HistoryId, cancellationToken);

        if (scanHistory == null)
            throw new NotFoundException("Scan history", query.HistoryId);

        // Verify ownership (users can only view their own AI summaries, admins can view any)
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.RequestingUserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", query.RequestingUserId);

        // Security: Return 404 instead of 403 to not leak resource existence
        if (user.UserType != UserType.Admin && scanHistory.UserId != user.UserId)
            throw new NotFoundException("Scan history", query.HistoryId);

        // Get AI summary using internal HistoryId (may be null - not all scans have AI summary)
        var iaSummary = await _unitOfWork.IASummaries.GetByHistoryIdAsync(scanHistory.HistoryId, cancellationToken);

        if (iaSummary == null)
            return null; // Expected - not all scans have AI summary

        // Map to response DTO
        return new IASummaryResponse(
            IASummaryId: iaSummary.IASummaryId,
            SummaryText: iaSummary.SummaryText,
            MainCategory: iaSummary.MainCategory,
            OverallRisk: iaSummary.OverallRisk,
            TotalFindings: iaSummary.TotalFindings,
            FindingsCritical: iaSummary.FindingsCritical,
            FindingsHigh: iaSummary.FindingsHigh,
            FindingsMedium: iaSummary.FindingsMedium,
            FindingsLow: iaSummary.FindingsLow,
            HistoryId: iaSummary.HistoryId,
            CreatedDate: iaSummary.CreatedDate
        );
    }
}
