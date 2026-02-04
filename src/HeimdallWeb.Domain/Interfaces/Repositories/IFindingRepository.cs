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
}
