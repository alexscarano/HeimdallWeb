using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Domain.ValueObjects;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity.
/// Uses EF Core with PostgreSQL for data access.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);
    }

    public async Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken ct = default)
    {
        // Convert EmailAddress VO to string for EF query
        var emailValue = email.Value;

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == emailValue, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync(ct);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        await _context.Users.AddAsync(user, ct);
        // SaveChanges will be called by UnitOfWork

        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        _context.Users.Update(user);
        // SaveChanges will be called by UnitOfWork

        await Task.CompletedTask;
    }

    public async Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken ct = default)
    {
        var emailValue = email.Value;

        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == emailValue, ct);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == username, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, int excludeUserId, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username && u.UserId != excludeUserId, ct);
    }

    public async Task<User?> GetByEmailAsync(EmailAddress email, int excludeUserId, CancellationToken ct = default)
    {
        var emailValue = email.Value;

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == emailValue && u.UserId != excludeUserId, ct);
    }

    public async Task DeleteAsync(int userId, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, ct);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
        // SaveChanges will be called by UnitOfWork
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPaginatedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        bool? isAdmin = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        CancellationToken ct = default)
    {
        var query = _context.Users.AsNoTracking();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(searchLower) ||
                u.Email.Value.ToLower().Contains(searchLower));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (isAdmin.HasValue)
        {
            var userType = isAdmin.Value ? Domain.Enums.UserType.Admin : Domain.Enums.UserType.Default;
            query = query.Where(u => u.UserType == userType);
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= createdTo.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(ct);

        // Apply pagination and ordering
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (users, totalCount);
    }
}
