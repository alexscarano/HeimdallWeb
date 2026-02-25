using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for RiskWeight entity.
/// Used by ScoreCalculatorService to load category multipliers from tb_risk_weights.
/// </summary>
public class RiskWeightRepository : IRiskWeightRepository
{
    private readonly AppDbContext _context;

    public RiskWeightRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RiskWeight>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _context.RiskWeights
            .AsNoTracking()
            .Where(rw => rw.IsActive)
            .OrderBy(rw => rw.Category)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<RiskWeight?> GetByCategoryAsync(string category, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be empty.", nameof(category));

        return await _context.RiskWeights
            .AsNoTracking()
            .FirstOrDefaultAsync(rw => rw.Category == category, ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RiskWeight>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.RiskWeights
            .AsNoTracking()
            .OrderBy(rw => rw.Category)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<RiskWeight> AddAsync(RiskWeight riskWeight, CancellationToken ct = default)
    {
        if (riskWeight == null)
            throw new ArgumentNullException(nameof(riskWeight));

        await _context.RiskWeights.AddAsync(riskWeight, ct);
        // SaveChanges will be called by UnitOfWork

        return riskWeight;
    }
}
