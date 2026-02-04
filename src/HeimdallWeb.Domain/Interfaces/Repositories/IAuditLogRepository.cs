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
}
