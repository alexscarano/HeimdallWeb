using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for MonitoredTarget aggregate.
/// </summary>
public interface IMonitoredTargetRepository
{
    /// <summary>Returns a monitored target by its primary key, or null if not found.</summary>
    Task<MonitoredTarget?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Returns all monitored targets registered by a specific user.</summary>
    Task<IEnumerable<MonitoredTarget>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Returns all active monitored targets whose <c>NextCheck</c> is in the past or present.
    /// Used by <see cref="HeimdallWeb.Application.Workers.MonitoringWorker"/> to select work items.
    /// </summary>
    Task<IEnumerable<MonitoredTarget>> GetDueForCheckAsync(CancellationToken ct = default);

    /// <summary>Persists a new monitored target and returns the tracked entity.</summary>
    Task<MonitoredTarget> AddAsync(MonitoredTarget target, CancellationToken ct = default);

    /// <summary>Removes the given monitored target from the database.</summary>
    Task DeleteAsync(MonitoredTarget target, CancellationToken ct = default);

    /// <summary>Marks a monitored target entity as modified so EF Core persists its changes.</summary>
    Task UpdateAsync(MonitoredTarget target, CancellationToken ct = default);

    /// <summary>
    /// Returns true when the user already has a monitored target registered for the given URL.
    /// Used to prevent duplicate registrations.
    /// </summary>
    Task<bool> ExistsByUserAndUrlAsync(int userId, string url, CancellationToken ct = default);
}
