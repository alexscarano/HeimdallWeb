using HeimdallWeb.Domain.DTOs;
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
    /// Gets a scan history record by its public UUID.
    /// </summary>
    Task<ScanHistory?> GetByPublicIdAsync(Guid publicId, CancellationToken ct = default);

    /// <summary>
    /// Gets a scan history record by its public UUID with all includes.
    /// </summary>
    Task<ScanHistory?> GetByPublicIdWithIncludesAsync(Guid publicId, CancellationToken ct = default);

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

    /// <summary>
    /// Deletes a scan history record by its unique identifier.
    /// </summary>
    Task DeleteAsync(int historyId, CancellationToken ct = default);

    /// <summary>
    /// Gets a scan history record with all includes (Findings, Technologies, IASummary).
    /// </summary>
    Task<ScanHistory?> GetByIdWithIncludesAsync(int historyId, CancellationToken ct = default);

    /// <summary>
    /// Gets paginated scan histories for a specific user, ordered by created date DESC.
    /// </summary>
    Task<(IEnumerable<ScanHistorySummaryResponse> Items, int TotalCount)> GetByUserIdPaginatedAsync(
        int userId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all scan histories with includes for PDF export.
    /// </summary>
    Task<IEnumerable<ScanHistory>> GetAllByUserIdWithIncludesAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets all scan histories in the system (admin only).
    /// </summary>
    Task<IEnumerable<ScanHistory>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Counts total scans for a specific user.
    /// </summary>
    Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets recent scan activities (for admin dashboard).
    /// </summary>
    /// <param name="count">Number of recent scans to retrieve</param>
    /// <param name="ct">Cancellation token</param>
    Task<IEnumerable<ScanHistory>> GetRecentAsync(int count, CancellationToken ct = default);
}
