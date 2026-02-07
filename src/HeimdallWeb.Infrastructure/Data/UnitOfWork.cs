using HeimdallWeb.Domain.Interfaces;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace HeimdallWeb.Infrastructure.Data;

/// <summary>
/// Unit of Work implementation for managing transactions and repository access.
/// Coordinates all database operations through a single DbContext instance.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy-loaded repositories
    private IUserRepository? _users;
    private IScanHistoryRepository? _scanHistories;
    private IFindingRepository? _findings;
    private ITechnologyRepository? _technologies;
    private IIASummaryRepository? _iaSummaries;
    private IAuditLogRepository? _auditLogs;
    private IUserUsageRepository? _userUsages;

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Repository Properties (lazy initialization)
    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IScanHistoryRepository ScanHistories =>
        _scanHistories ??= new ScanHistoryRepository(_context);

    public IFindingRepository Findings =>
        _findings ??= new FindingRepository(_context);

    public ITechnologyRepository Technologies =>
        _technologies ??= new TechnologyRepository(_context);

    public IIASummaryRepository IASummaries =>
        _iaSummaries ??= new IASummaryRepository(_context);

    public IAuditLogRepository AuditLogs =>
        _auditLogs ??= new AuditLogRepository(_context);

    public IUserUsageRepository UserUsages =>
        _userUsages ??= new UserUsageRepository(_context);

    // Transaction Methods
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Executes a transactional operation with retry support.
    /// Use this method instead of manual BeginTransaction/Commit/Rollback.
    /// </summary>
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(
            state: operation,
            operation: async (dbContext, op, cancellationToken) =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await op(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            },
            verifySucceeded: null,
            cancellationToken: ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction in progress.");

        try
        {
            await _context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction in progress.");

        try
        {
            await _transaction.RollbackAsync(ct);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // IDisposable Implementation
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
