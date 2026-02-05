using HeimdallWeb.Domain.Interfaces.Repositories;

namespace HeimdallWeb.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions and repository access.
/// Provides a single point of coordination for all repository operations.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository Properties
    IUserRepository Users { get; }
    IScanHistoryRepository ScanHistories { get; }
    IFindingRepository Findings { get; }
    ITechnologyRepository Technologies { get; }
    IIASummaryRepository IASummaries { get; }
    IAuditLogRepository AuditLogs { get; }
    IUserUsageRepository UserUsages { get; }

    // Transaction Methods
    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <returns>The number of affected rows</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
