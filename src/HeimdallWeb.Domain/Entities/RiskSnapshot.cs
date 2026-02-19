using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// Immutable snapshot of a target's security posture at a specific point in time.
/// Created after each monitoring scan to enable trend analysis and regression detection.
/// </summary>
public class RiskSnapshot
{
    /// <summary>Primary key (auto-generated).</summary>
    public int Id { get; private set; }

    /// <summary>FK referencing the monitored target that produced this snapshot.</summary>
    public int MonitoredTargetId { get; private set; }

    /// <summary>FK referencing the ScanHistory record generated for this snapshot.</summary>
    public int ScanHistoryId { get; private set; }

    /// <summary>Security score from 0 (worst) to 100 (best).</summary>
    public int Score { get; private set; }

    /// <summary>Letter grade: A (90+), B (80–89), C (70–79), D (60–69), F (&lt;60).</summary>
    public string Grade { get; private set; } = string.Empty;

    /// <summary>Total number of findings discovered in this scan.</summary>
    public int FindingsCount { get; private set; }

    /// <summary>Number of Critical-severity findings.</summary>
    public int CriticalCount { get; private set; }

    /// <summary>Number of High-severity findings.</summary>
    public int HighCount { get; private set; }

    /// <summary>When this snapshot was captured.</summary>
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    /// <summary>Monitored target this snapshot belongs to.</summary>
    public MonitoredTarget? MonitoredTarget { get; private set; }

    /// <summary>ScanHistory record that generated this snapshot.</summary>
    public ScanHistory? ScanHistory { get; private set; }

    // Parameterless constructor for EF Core
    private RiskSnapshot() { }

    /// <summary>
    /// Creates a new RiskSnapshot from a completed monitoring scan.
    /// </summary>
    /// <param name="monitoredTargetId">FK to the monitored target.</param>
    /// <param name="scanHistoryId">FK to the ScanHistory record.</param>
    /// <param name="score">Security score (0–100).</param>
    /// <param name="grade">Letter grade (A–F).</param>
    /// <param name="findingsCount">Total findings count.</param>
    /// <param name="criticalCount">Critical findings count.</param>
    /// <param name="highCount">High findings count.</param>
    public RiskSnapshot(
        int monitoredTargetId,
        int scanHistoryId,
        int score,
        string grade,
        int findingsCount,
        int criticalCount,
        int highCount)
    {
        if (monitoredTargetId <= 0)
            throw new ValidationException("MonitoredTargetId must be a positive integer.");

        if (scanHistoryId <= 0)
            throw new ValidationException("ScanHistoryId must be a positive integer.");

        if (score < 0 || score > 100)
            throw new ValidationException("Score must be between 0 and 100.");

        if (string.IsNullOrWhiteSpace(grade))
            throw new ValidationException("Grade cannot be empty.");

        MonitoredTargetId = monitoredTargetId;
        ScanHistoryId = scanHistoryId;
        Score = score;
        Grade = grade;
        FindingsCount = findingsCount;
        CriticalCount = criticalCount;
        HighCount = highCount;
        CreatedAt = DateTime.UtcNow;
    }
}
