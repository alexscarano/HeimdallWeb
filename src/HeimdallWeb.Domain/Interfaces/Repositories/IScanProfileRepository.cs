using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ScanProfile entities.
/// </summary>
public interface IScanProfileRepository
{
    /// <summary>Returns all scan profiles ordered by name.</summary>
    Task<IEnumerable<ScanProfile>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns a scan profile by its primary key, or null when not found.</summary>
    Task<ScanProfile?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Returns a scan profile by its unique name (case-insensitive), or null when not found.</summary>
    Task<ScanProfile?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>Adds a new scan profile. SaveChanges must be called by UnitOfWork.</summary>
    Task<ScanProfile> AddAsync(ScanProfile scanProfile, CancellationToken ct = default);
}
