using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.User.GetUserStatistics;

/// <summary>
/// Handler for GetUserStatisticsQuery.
/// Calculates comprehensive user statistics including scans, findings, and trends.
/// Source: HeimdallWebOld/Controllers/UserController.cs lines 59-70 (Statistics method)
/// </summary>
public class GetUserStatisticsQueryHandler : IQueryHandler<GetUserStatisticsQuery, UserStatisticsResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserStatisticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<UserStatisticsResponse> Handle(GetUserStatisticsQuery query, CancellationToken cancellationToken = default)
    {
        // Verify user exists and resolve to internal ID
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", query.UserId);

        // Verify ownership: users can only view their own statistics, admins can view any
        var requestingUser = await _unitOfWork.Users.GetByPublicIdAsync(query.RequestingUserId, cancellationToken);
        if (requestingUser == null)
            throw new NotFoundException("User", query.RequestingUserId);

        // Security: Return 404 instead of 403 to not leak resource existence
        if (requestingUser.UserType != UserType.Admin && query.UserId != query.RequestingUserId)
            throw new NotFoundException("User", query.UserId);

        var userInternalId = user.UserId; // Use internal ID for FK queries

        // Get all scan histories for user
        var scanHistories = (await _unitOfWork.ScanHistories.GetByUserIdAsync(userInternalId, cancellationToken)).ToList();

        // Calculate scan statistics
        var totalScans = scanHistories.Count;
        var completedScans = scanHistories.Count(h => h.HasCompleted);
        var incompleteScans = totalScans - completedScans;

        // Calculate average duration (only for completed scans with duration)
        string? averageDuration = null;
        var completedWithDuration = scanHistories.Where(h => h.HasCompleted && h.Duration != null).ToList();
        if (completedWithDuration.Any())
        {
            var totalSeconds = completedWithDuration.Sum(h => ((TimeSpan)h.Duration!).TotalSeconds);
            var avgSeconds = totalSeconds / completedWithDuration.Count;
            var avgTimeSpan = TimeSpan.FromSeconds(avgSeconds);
            averageDuration = avgTimeSpan.ToString(@"hh\:mm\:ss");
        }

        // Get last scan date
        string? lastScanDate = scanHistories.Any()
            ? scanHistories.Max(h => h.CreatedDate).ToString("o") // ISO 8601 format
            : null;

        // Get all findings for user
        var findings = (await _unitOfWork.Findings.GetByUserIdAsync(userInternalId, cancellationToken)).ToList();

        // Calculate findings statistics
        var totalFindings = findings.Count;
        var criticalFindings = findings.Count(f => f.Severity == SeverityLevel.Critical);
        var highFindings = findings.Count(f => f.Severity == SeverityLevel.High);
        var mediumFindings = findings.Count(f => f.Severity == SeverityLevel.Medium);
        var lowFindings = findings.Count(f => f.Severity == SeverityLevel.Low);
        var informationalFindings = findings.Count(f => f.Severity == SeverityLevel.Informational);

        // Risk trend from SQL VIEW (last 30 days)
        var riskTrendData = await _unitOfWork.UserStatisticsViews.GetUserRiskTrendAsync(userInternalId, cancellationToken);
        var riskTrend = riskTrendData.Select(r => new RiskTrendItem(
            Date: r.RiskDate.ToString("yyyy-MM-dd"),
            FindingsCount: r.CriticalCount + r.HighCount + r.MediumCount + r.LowCount + r.InformationalCount
        )).ToList();

        // Category breakdown from SQL VIEW
        var categoryData = await _unitOfWork.UserStatisticsViews.GetUserCategoryBreakdownAsync(userInternalId, cancellationToken);
        var categoryBreakdown = categoryData.Select(c => new CategoryBreakdownItem(
            Category: c.Category,
            Count: c.CategoryCount
        )).ToList();

        return new UserStatisticsResponse(
            TotalScans: totalScans,
            CompletedScans: completedScans,
            IncompleteScans: incompleteScans,
            AverageDuration: averageDuration,
            LastScanDate: lastScanDate,
            TotalFindings: totalFindings,
            CriticalFindings: criticalFindings,
            HighFindings: highFindings,
            MediumFindings: mediumFindings,
            LowFindings: lowFindings,
            InformationalFindings: informationalFindings,
            RiskTrend: riskTrend,
            CategoryBreakdown: categoryBreakdown
        );
    }
}
