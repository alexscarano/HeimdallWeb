namespace HeimdallWeb.Application.Queries.Admin.GetAdminDashboard;

/// <summary>
/// Query to retrieve admin dashboard statistics.
/// Requires Admin role. Returns comprehensive system statistics.
/// Source: HeimdallWebOld/Controllers/AdminController.cs lines 36-64 (Dashboard method)
/// </summary>
/// <param name="RequestingUserId">The user requesting dashboard (for admin verification)</param>
/// <param name="LogPage">Log pagination page (default 1)</param>
/// <param name="LogPageSize">Log pagination page size (default 10, max 50)</param>
/// <param name="LogLevel">Filter logs by level (optional)</param>
/// <param name="LogStartDate">Filter logs by start date (optional)</param>
/// <param name="LogEndDate">Filter logs by end date (optional)</param>
public record GetAdminDashboardQuery(
    Guid RequestingUserId,
    int LogPage = 1,
    int LogPageSize = 10,
    string? LogLevel = null,
    DateTime? LogStartDate = null,
    DateTime? LogEndDate = null
);
