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
        // Verify user exists
        var user = await _unitOfWork.Users.GetByIdAsync(query.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", query.UserId);

        // Get all scan histories for user
        var scanHistories = (await _unitOfWork.ScanHistories.GetByUserIdAsync(query.UserId, cancellationToken)).ToList();

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
        var findings = (await _unitOfWork.Findings.GetByUserIdAsync(query.UserId, cancellationToken)).ToList();

        // Calculate findings statistics
        var totalFindings = findings.Count;
        var criticalFindings = findings.Count(f => f.Severity == SeverityLevel.Critical);
        var highFindings = findings.Count(f => f.Severity == SeverityLevel.High);
        var mediumFindings = findings.Count(f => f.Severity == SeverityLevel.Medium);
        var lowFindings = findings.Count(f => f.Severity == SeverityLevel.Low);
        var informationalFindings = findings.Count(f => f.Severity == SeverityLevel.Informational);

        // Risk trend and category breakdown (simplified - return empty lists for Phase 3)
        // Can be enhanced later with SQL VIEWs or more complex queries
        var riskTrend = new List<RiskTrendItem>();
        var categoryBreakdown = new List<CategoryBreakdownItem>();

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
