using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ScanProfile entity.
/// Provides read and create operations for scan profiles stored in tb_scan_profile.
/// </summary>
public class ScanProfileRepository : IScanProfileRepository
{
    private readonly AppDbContext _context;

    public ScanProfileRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ScanProfile>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ScanProfiles
            .AsNoTracking()
            .OrderBy(sp => sp.Name)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ScanProfile?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.ScanProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<ScanProfile?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return await _context.ScanProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Name.ToLower() == name.ToLower(), ct);
    }

    /// <inheritdoc/>
    public async Task<ScanProfile> AddAsync(ScanProfile scanProfile, CancellationToken ct = default)
    {
        if (scanProfile == null)
            throw new ArgumentNullException(nameof(scanProfile));

        await _context.ScanProfiles.AddAsync(scanProfile, ct);
        // SaveChanges will be called by UnitOfWork

        return scanProfile;
    }
}
