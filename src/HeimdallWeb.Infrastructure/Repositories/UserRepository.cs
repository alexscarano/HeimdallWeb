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
}
