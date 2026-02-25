using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for RiskWeight entities.
/// </summary>
public interface IRiskWeightRepository
{
    /// <summary>Returns all active risk weights.</summary>
    Task<IEnumerable<RiskWeight>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>Returns a risk weight by category (case-insensitive).</summary>
    Task<RiskWeight?> GetByCategoryAsync(string category, CancellationToken ct = default);

    /// <summary>Returns all risk weights (active and inactive).</summary>
    Task<IEnumerable<RiskWeight>> GetAllAsync(CancellationToken ct = default);

    Task<RiskWeight> AddAsync(RiskWeight riskWeight, CancellationToken ct = default);
}
