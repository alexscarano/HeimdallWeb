using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Technology entity operations.
/// </summary>
public interface ITechnologyRepository
{
    /// <summary>
    /// Gets all technologies detected for a specific scan history.
    /// </summary>
    Task<IEnumerable<Technology>> GetByHistoryIdAsync(int historyId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new technology.
    /// </summary>
    Task<Technology> AddAsync(Technology technology, CancellationToken ct = default);

    /// <summary>
    /// Adds multiple technologies in a single operation.
    /// </summary>
    Task AddRangeAsync(IEnumerable<Technology> technologies, CancellationToken ct = default);
}
