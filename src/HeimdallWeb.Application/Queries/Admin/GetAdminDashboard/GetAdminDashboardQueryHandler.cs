using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Admin;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Admin.GetAdminDashboard;

/// <summary>
/// Handler for GetAdminDashboardQuery.
/// Returns comprehensive admin dashboard statistics (admin only).
/// Source: HeimdallWebOld/Controllers/AdminController.cs lines 36-64 (Dashboard method)
/// </summary>
public class GetAdminDashboardQueryHandler : IQueryHandler<GetAdminDashboardQuery, AdminDashboardResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<AdminDashboardResponse> Handle(GetAdminDashboardQuery query, CancellationToken cancellationToken = default)
    {
        // Verify requesting user is admin
        var requestingUser = await _unitOfWork.Users.GetByIdAsync(query.RequestingUserId, cancellationToken);
        if (requestingUser == null)
            throw new NotFoundException("User", query.RequestingUserId);

        if (requestingUser.UserType != UserType.Admin)
            throw new ForbiddenException("Admin access required");

        // Validate and cap page size
        var pageSize = Math.Min(query.LogPageSize, 50);
        if (pageSize <= 0) pageSize = 10;

        // Get all users
        var users = (await _unitOfWork.Users.GetAllAsync(cancellationToken)).ToList();

        // Calculate user statistics
        var userStats = new UserStatsSection(
            TotalUsers: users.Count,
            ActiveUsers: users.Count(u => u.IsActive),
            BlockedUsers: users.Count(u => !u.IsActive),
            AdminUsers: users.Count(u => u.UserType == UserType.Admin),
            RegularUsers: users.Count(u => u.UserType == UserType.Default)
        );

        // Get all scan histories
        var scanHistories = (await _unitOfWork.ScanHistories.GetAllAsync(cancellationToken)).ToList();

        // Get all findings
        var findings = (await _unitOfWork.Findings.GetAllAsync(cancellationToken)).ToList();

        // Calculate scan statistics
        var scanStats = new ScanStatsSection(
            TotalScans: scanHistories.Count,
            CompletedScans: scanHistories.Count(h => h.HasCompleted),
            IncompleteScans: scanHistories.Count(h => !h.HasCompleted),
            TotalFindings: findings.Count,
            CriticalFindings: findings.Count(f => f.Severity == SeverityLevel.Critical),
            HighFindings: findings.Count(f => f.Severity == SeverityLevel.High),
            MediumFindings: findings.Count(f => f.Severity == SeverityLevel.Medium),
            LowFindings: findings.Count(f => f.Severity == SeverityLevel.Low)
        );

        // Get paginated logs
        var (logs, totalLogCount) = await _unitOfWork.AuditLogs.GetPaginatedAsync(
            query.LogPage,
            pageSize,
            query.LogLevel,
            query.LogStartDate,
            query.LogEndDate,
            cancellationToken);

        var logItems = logs.Select(l => new LogItem(
            LogId: l.LogId,
            Timestamp: l.Timestamp,
            Level: l.Level,
            Source: l.Source ?? string.Empty,
            Message: l.Message,
            UserId: l.UserId,
            Username: l.User?.Username,
            RemoteIp: l.RemoteIp
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalLogCount / (double)pageSize);

        var paginatedLogs = new PaginatedLogsSection(
            Items: logItems,
            Page: query.LogPage,
            PageSize: pageSize,
            TotalCount: totalLogCount,
            TotalPages: totalPages
        );

        // Get recent activity (last 10 scans)
        var recentScans = await _unitOfWork.ScanHistories.GetRecentAsync(10, cancellationToken);

        var recentActivity = recentScans.Select(h => new RecentActivityItem(
            HistoryId: h.HistoryId,
            Target: h.Target.Value,
            CreatedDate: h.CreatedDate,
            UserId: h.UserId,
            Username: h.User?.Username ?? "Unknown",
            HasCompleted: h.HasCompleted,
            FindingsCount: h.Findings.Count
        )).ToList();

        // Trends (simplified - return empty lists for Phase 3)
        // Can be enhanced later with SQL VIEWs
        var scanTrend = new List<TrendItem>();
        var userRegistrationTrend = new List<TrendItem>();

        return new AdminDashboardResponse(
            UserStats: userStats,
            ScanStats: scanStats,
            Logs: paginatedLogs,
            RecentActivity: recentActivity,
            ScanTrend: scanTrend,
            UserRegistrationTrend: userRegistrationTrend
        );
    }
}
