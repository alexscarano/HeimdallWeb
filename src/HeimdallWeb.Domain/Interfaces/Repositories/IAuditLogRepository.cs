using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for AuditLog entity operations.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Adds a new audit log entry.
    /// </summary>
    Task<AuditLog> AddAsync(AuditLog log, CancellationToken ct = default);

    /// <summary>
    /// Gets the most recent audit log entries.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated audit logs with optional filters.
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="level">Filter by log level (optional)</param>
    /// <param name="startDate">Filter by start date (optional)</param>
    /// <param name="endDate">Filter by end date (optional)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Tuple with logs list and total count</returns>
    Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPaginatedAsync(
        int page,
        int pageSize,
        string? level = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken ct = default);
}
