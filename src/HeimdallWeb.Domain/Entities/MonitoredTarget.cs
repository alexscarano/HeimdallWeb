using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// Represents a URL that the system monitors periodically for security regressions.
/// Each user can register multiple monitored targets with a configurable scan frequency.
/// </summary>
public class MonitoredTarget
{
    /// <summary>Primary key (auto-generated).</summary>
    public int Id { get; private set; }

    /// <summary>Internal FK referencing the owning user (tb_user.user_id).</summary>
    public int UserId { get; private set; }

    /// <summary>The URL being monitored. Max 2048 characters.</summary>
    public string Url { get; private set; } = string.Empty;

    /// <summary>How often the target is scanned (Daily or Weekly).</summary>
    public MonitorFrequency Frequency { get; private set; }

    /// <summary>Date and time of the last completed check. Null until first check runs.</summary>
    public DateTime? LastCheck { get; private set; }

    /// <summary>Date and time when the next check should be triggered by the worker.</summary>
    public DateTime NextCheck { get; private set; }

    /// <summary>Whether monitoring is currently active for this target.</summary>
    public bool IsActive { get; private set; }

    /// <summary>When this monitoring target was registered.</summary>
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    /// <summary>User that owns this monitored target.</summary>
    public User? User { get; private set; }

    /// <summary>All risk snapshots collected for this target.</summary>
    public ICollection<RiskSnapshot> RiskSnapshots { get; private set; } = new List<RiskSnapshot>();

    // Parameterless constructor for EF Core
    private MonitoredTarget() { }

    /// <summary>
    /// Creates a new MonitoredTarget for the specified user.
    /// </summary>
    /// <param name="userId">Internal user ID (FK to tb_user).</param>
    /// <param name="url">URL to monitor. Must be non-empty.</param>
    /// <param name="frequency">Scan frequency (Daily or Weekly).</param>
    public MonitoredTarget(int userId, string url, MonitorFrequency frequency)
    {
        if (userId <= 0)
            throw new ValidationException("UserId must be a positive integer.");

        if (string.IsNullOrWhiteSpace(url))
            throw new ValidationException("URL cannot be empty.");

        if (url.Length > 2048)
            throw new ValidationException("URL cannot exceed 2048 characters.");

        UserId = userId;
        Url = url.Trim();
        Frequency = frequency;

        // Se a NextCheck in the past so the background worker picks it up immediately
        NextCheck = DateTime.UtcNow.AddMinutes(-1);

        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates monitoring for this target.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates monitoring for this target. The worker will skip it until re-activated.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Records the outcome of a completed check and schedules the next one.
    /// </summary>
    /// <param name="lastCheck">Timestamp when the check completed.</param>
    /// <param name="nextCheck">Timestamp for the subsequent check.</param>
    public void UpdateSchedule(DateTime lastCheck, DateTime nextCheck)
    {
        LastCheck = lastCheck;
        NextCheck = nextCheck;
    }

    /// <summary>
    /// Changes the scan frequency and recalculates <see cref="NextCheck"/> from now.
    /// </summary>
    /// <param name="frequency">New scan frequency.</param>
    public void UpdateFrequency(MonitorFrequency frequency)
    {
        Frequency = frequency;
        NextCheck = DateTime.UtcNow.AddDays((int)frequency);
    }
}
