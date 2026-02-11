using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Scan.GetScanHistoryById;

/// <summary>
/// Handler for GetScanHistoryByIdQuery.
/// Retrieves a scan history with full details (findings, technologies, AI summary).
/// Validates ownership before returning data (users can only view their own scans, admins can view any).
///
/// Source: HeimdallWebOld/Controllers/HistoryController.cs lines 66-88 (ViewJson method)
/// </summary>
public class GetScanHistoryByIdQueryHandler : IQueryHandler<GetScanHistoryByIdQuery, ScanHistoryDetailResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetScanHistoryByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ScanHistoryDetailResponse> Handle(GetScanHistoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        // Get scan history with all includes by PublicId
        var scanHistory = await _unitOfWork.ScanHistories.GetByPublicIdWithIncludesAsync(query.HistoryId, cancellationToken);

        if (scanHistory == null)
            throw new NotFoundException("Scan history", query.HistoryId);

        // Verify ownership (users can only view their own scans, admins can view any)
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.RequestingUserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", query.RequestingUserId);

        // Security: Return 404 instead of 403 to not leak resource existence
        if (user.UserType != UserType.Admin && scanHistory.UserId != user.UserId)
            throw new NotFoundException("Scan history", query.HistoryId);

        // Get related entities using internal HistoryId
        var findings = await _unitOfWork.Findings.GetByHistoryIdAsync(scanHistory.HistoryId, cancellationToken);
        var technologies = await _unitOfWork.Technologies.GetByHistoryIdAsync(scanHistory.HistoryId, cancellationToken);
        var iaSummary = await _unitOfWork.IASummaries.GetByHistoryIdAsync(scanHistory.HistoryId, cancellationToken);

        // Map to response DTOs
        var findingResponses = findings.Select(f => new FindingResponse(
            FindingId: f.FindingId,
            Type: f.Type,
            Description: f.Description,
            Severity: f.Severity.ToString(),
            Evidence: f.Evidence,
            Recommendation: f.Recommendation,
            HistoryId: f.HistoryId,
            CreatedAt: f.CreatedAt
        ));

        var technologyResponses = technologies.Select(t => new TechnologyResponse(
            TechnologyId: t.TechnologyId,
            Name: t.Name,
            Version: t.Version,
            Category: t.Category,
            Description: t.Description,
            HistoryId: t.HistoryId,
            CreatedAt: t.CreatedAt
        ));

        IASummaryResponse? iaSummaryResponse = null;
        if (iaSummary != null)
        {
            iaSummaryResponse = new IASummaryResponse(
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

        // Convert ScanDuration to string format
        string? durationString = null;
        if (scanHistory.Duration != null)
        {
            TimeSpan duration = scanHistory.Duration; // Implicit conversion from ScanDuration to TimeSpan
            durationString = duration.ToString(@"hh\:mm\:ss");
        }

        return new ScanHistoryDetailResponse(
            HistoryId: scanHistory.PublicId,
            Target: scanHistory.Target.Value,
            RawJsonResult: scanHistory.RawJsonResult,
            CreatedDate: scanHistory.CreatedDate,
            UserId: scanHistory.User?.PublicId ?? Guid.Empty,
            Duration: durationString,
            HasCompleted: scanHistory.HasCompleted,
            Summary: scanHistory.Summary,
            Findings: findingResponses,
            Technologies: technologyResponses,
            IASummary: iaSummaryResponse
        );
    }
}
