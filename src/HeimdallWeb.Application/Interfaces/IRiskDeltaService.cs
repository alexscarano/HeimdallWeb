namespace HeimdallWeb.Application.Interfaces;

/// <summary>
/// Service for detecting security regressions between consecutive monitoring scans.
/// Compares the latest scan result against the previous <see cref="HeimdallWeb.Domain.Entities.RiskSnapshot"/>
/// and records an audit log entry when a critical score drop is detected.
/// </summary>
public interface IRiskDeltaService
{
    /// <summary>
    /// Saves a new <see cref="HeimdallWeb.Domain.Entities.RiskSnapshot"/> for the monitored target
    /// and compares it against the previous snapshot to detect critical regressions.
    /// If the score dropped by <c>10</c> or more points, an audit log entry is written
    /// at Critical level.
    /// </summary>
    /// <param name="monitoredTargetId">FK to the monitored target.</param>
    /// <param name="scanHistoryId">FK to the ScanHistory record generated for this scan.</param>
    /// <param name="newScore">Security score calculated for the latest scan (0–100).</param>
    /// <param name="newGrade">Letter grade for the latest scan (A–F).</param>
    /// <param name="findingsCount">Total findings in the latest scan.</param>
    /// <param name="criticalCount">Critical-severity findings in the latest scan.</param>
    /// <param name="highCount">High-severity findings in the latest scan.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when a critical regression was detected; otherwise false.</returns>
    Task<bool> DetectAndHandleDeltaAsync(
        int monitoredTargetId,
        int scanHistoryId,
        int newScore,
        string newGrade,
        int findingsCount,
        int criticalCount,
        int highCount,
        CancellationToken ct = default);
}
