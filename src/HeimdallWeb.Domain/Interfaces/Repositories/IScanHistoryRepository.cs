using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ScanHistory entity operations.
/// </summary>
public interface IScanHistoryRepository
{
    /// <summary>
    /// Gets a scan history record by its unique identifier.
    /// </summary>
    Task<ScanHistory?> GetByIdAsync(int historyId, CancellationToken ct = default);

    /// <summary>
    /// Gets all scan history records for a specific user.
    /// </summary>
    Task<IEnumerable<ScanHistory>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new scan history record.
    /// </summary>
    Task<ScanHistory> AddAsync(ScanHistory history, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing scan history record.
    /// </summary>
    Task UpdateAsync(ScanHistory history, CancellationToken ct = default);
}
