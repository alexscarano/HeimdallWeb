using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Finding entity operations.
/// </summary>
public interface IFindingRepository
{
    /// <summary>
    /// Gets all findings for a specific scan history.
    /// </summary>
    Task<IEnumerable<Finding>> GetByHistoryIdAsync(int historyId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new finding.
    /// </summary>
    Task<Finding> AddAsync(Finding finding, CancellationToken ct = default);

    /// <summary>
    /// Adds multiple findings in a single operation.
    /// </summary>
    Task AddRangeAsync(IEnumerable<Finding> findings, CancellationToken ct = default);

    /// <summary>
    /// Gets all findings for a specific user (across all their scan histories).
    /// </summary>
    Task<IEnumerable<Finding>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets all findings in the system (admin only).
    /// </summary>
    Task<IEnumerable<Finding>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Counts total findings for a specific user.
    /// </summary>
    Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default);
}
