using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces.Repositories;
using HeimdallWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Notification entity.
/// Uses EF Core with PostgreSQL for data access.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10, CancellationToken ct = default)
        => await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderBy(n => n.IsRead)
            .ThenByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
        => await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task<Notification?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Notifications.FindAsync(new object[] { id }, ct);

    public async Task<Notification> AddAsync(Notification notification, CancellationToken ct = default)
    {
        if (notification == null)
            throw new ArgumentNullException(nameof(notification));

        await _context.Notifications.AddAsync(notification, ct);
        // SaveChanges will be called by UnitOfWork
        return notification;
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken ct = default)
        => await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);
}
