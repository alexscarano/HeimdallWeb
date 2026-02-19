using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Application.Services;

/// <summary>
/// Implementation of <see cref="IRiskDeltaService"/>.
/// Persists a new RiskSnapshot after each monitoring scan and detects critical score regressions.
/// </summary>
public class RiskDeltaService : IRiskDeltaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RiskDeltaService> _logger;

    /// <summary>Minimum score drop (in points) that triggers a Critical audit log entry.</summary>
    private const int CriticalScoreDropThreshold = 10;

    public RiskDeltaService(IUnitOfWork unitOfWork, ILogger<RiskDeltaService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<bool> DetectAndHandleDeltaAsync(
        int monitoredTargetId,
        int scanHistoryId,
        int newScore,
        string newGrade,
        int findingsCount,
        int criticalCount,
        int highCount,
        CancellationToken ct = default)
    {
        // 1. Retrieve the most recent snapshot to use as baseline for comparison
        var previousSnapshot = await _unitOfWork.RiskSnapshots.GetLatestByTargetIdAsync(monitoredTargetId, ct);

        // 2. Persist the new snapshot
        var newSnapshot = new RiskSnapshot(
            monitoredTargetId: monitoredTargetId,
            scanHistoryId: scanHistoryId,
            score: newScore,
            grade: newGrade,
            findingsCount: findingsCount,
            criticalCount: criticalCount,
            highCount: highCount);

        await _unitOfWork.RiskSnapshots.AddAsync(newSnapshot, ct);

        bool criticalChange = false;

        // 3. Detect critical regression only when a previous baseline exists
        if (previousSnapshot != null)
        {
            int scoreDelta = previousSnapshot.Score - newScore;

            if (scoreDelta >= CriticalScoreDropThreshold)
            {
                var details = $"{{\"previousScore\":{previousSnapshot.Score},\"newScore\":{newScore},\"delta\":{scoreDelta}}}";

                var auditLog = new AuditLog(
                    code: LogEventCode.SCAN_COMPLETED,
                    level: "Critical",
                    message: $"Score caiu {scoreDelta} pontos para alvo monitorado ID {monitoredTargetId}",
                    source: "MonitoringWorker",
                    details: details,
                    userId: null,
                    historyId: scanHistoryId,
                    remoteIp: null);

                await _unitOfWork.AuditLogs.AddAsync(auditLog, ct);

                _logger.LogWarning(
                    "Critical score drop detected for MonitoredTarget {TargetId}: {OldScore} -> {NewScore} (delta: -{Delta})",
                    monitoredTargetId, previousSnapshot.Score, newScore, scoreDelta);

                criticalChange = true;
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return criticalChange;
    }
}
