namespace HeimdallWeb.Application.DTOs.Admin;

/// <summary>
/// Response DTO for admin dashboard with comprehensive system statistics.
/// Used in GetAdminDashboardQuery.
/// </summary>
public record AdminDashboardResponse(
    UserStatsSection UserStats,
    ScanStatsSection ScanStats,
    PaginatedLogsSection Logs,
    List<RecentActivityItem> RecentActivity,
    List<TrendItem> ScanTrend,
    List<TrendItem> UserRegistrationTrend
);

/// <summary>
/// User statistics section.
/// </summary>
public record UserStatsSection(
    int TotalUsers,
    int ActiveUsers,
    int BlockedUsers,
    int AdminUsers,
    int RegularUsers
);

/// <summary>
/// Scan statistics section.
/// </summary>
public record ScanStatsSection(
    int TotalScans,
    int CompletedScans,
    int IncompleteScans,
    int TotalFindings,
    int CriticalFindings,
    int HighFindings,
    int MediumFindings,
    int LowFindings
);

/// <summary>
/// Paginated logs section with metadata.
/// </summary>
public record PaginatedLogsSection(
    List<LogItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

/// <summary>
/// Individual log item.
/// </summary>
public record LogItem(
    int LogId,
    DateTime Timestamp,
    string Level,
    string Source,
    string Message,
    int? UserId,
    string? Username,
    string? RemoteIp
);

/// <summary>
/// Recent scan activity item.
/// </summary>
public record RecentActivityItem(
    int HistoryId,
    string Target,
    DateTime CreatedDate,
    int UserId,
    string Username,
    bool HasCompleted,
    int FindingsCount
);

/// <summary>
/// Trend data item (date + count).
/// </summary>
public record TrendItem(
    string Date,
    int Count
);
