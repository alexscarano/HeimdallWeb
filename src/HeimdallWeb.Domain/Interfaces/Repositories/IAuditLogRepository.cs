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
    Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPaginatedAsync(
        int page,
        int pageSize,
        string? level = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? source = null,
        string? username = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all distinct Source values for filter dropdown.
    /// </summary>
    Task<List<string>> GetDistinctSourcesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all distinct Message values for filter dropdown.
    /// </summary>
    Task<List<string>> GetDistinctMessagesAsync(CancellationToken ct = default);
}
