using HeimdallWeb.Application.Commands.Scan.ExecuteScan;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Application.Workers;

/// <summary>
/// Background service that periodically runs security scans against registered monitored targets.
/// Executes every 30 minutes, picks up targets whose <c>NextCheck</c> timestamp is in the past,
/// runs a full scan via <see cref="ExecuteScanCommandHandler"/>, and delegates risk delta
/// detection to <see cref="IRiskDeltaService"/>.
/// </summary>
public class MonitoringWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonitoringWorker> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(30);

    public MonitoringWorker(IServiceProvider serviceProvider, ILogger<MonitoringWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MonitoringWorker started.");

        using var timer = new PeriodicTimer(CheckInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCheckAsync(stoppingToken);
        }

        _logger.LogInformation("MonitoringWorker stopping.");
    }

    private async Task RunCheckAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var scanHandler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExecuteScanCommand, ExecuteScanResponse>>();
        var riskDeltaService = scope.ServiceProvider.GetRequiredService<IRiskDeltaService>();
        var scanCacheService = scope.ServiceProvider.GetRequiredService<IScanCacheService>();

        try
        {
            // Cleanup expired cache entries as a periodic housekeeping task
            await scanCacheService.CleanupExpiredCacheAsync(ct);

            var dueTargets = await unitOfWork.MonitoredTargets.GetDueForCheckAsync(ct);
            var targetList = dueTargets.ToList();

            if (targetList.Count == 0)
            {
                _logger.LogDebug("MonitoringWorker: no targets due for check at {Time}.", DateTime.UtcNow);
                return;
            }

            _logger.LogInformation("MonitoringWorker: processing {Count} due target(s).", targetList.Count);

            foreach (var target in targetList)
            {
                await ProcessTargetAsync(target, unitOfWork, scanHandler, riskDeltaService, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the host is shutting down — no action needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MonitoringWorker.RunCheckAsync encountered an unhandled error.");
        }
    }

    private async Task ProcessTargetAsync(
        MonitoredTarget target,
        IUnitOfWork unitOfWork,
        ICommandHandler<ExecuteScanCommand, ExecuteScanResponse> scanHandler,
        IRiskDeltaService riskDeltaService,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "MonitoringWorker: processing target {TargetId} ({Url}).",
            target.Id, target.Url);

        try
        {
            // Resolve user PublicId (required by ExecuteScanCommand) from the stored internal UserId
            var user = await unitOfWork.Users.GetByIdAsync(target.UserId, ct);
            if (user == null)
            {
                _logger.LogWarning(
                    "MonitoringWorker: user {UserId} not found for target {TargetId}. Skipping.",
                    target.UserId, target.Id);
                return;
            }

            // Execute the full scan by reusing the existing command handler.
            // Use the loopback address as a valid sentinel IP for internal worker scans.
            // This satisfies the ExecuteScanCommandValidator's IP format check while
            // clearly identifying the request as originating from the monitoring subsystem.
            var command = new ExecuteScanCommand(
                Target: target.Url,
                UserId: user.PublicId,
                RemoteIp: "127.0.0.1",
                ProfileId: null,
                EnabledScanners: null);

            var scanResult = await scanHandler.Handle(command, ct);

            // If scan did not complete (timeout / error), update schedule but skip snapshot
            if (!scanResult.HasCompleted)
            {
                _logger.LogWarning(
                    "MonitoringWorker: scan for target {TargetId} did not complete. Score will not be snapshotted.",
                    target.Id);

                await UpdateTargetScheduleAsync(target, unitOfWork, ct);
                return;
            }

            // Resolve the ScanHistory internal ID from the PublicId returned by the handler
            var history = await unitOfWork.ScanHistories.GetByPublicIdAsync(scanResult.HistoryId, ct);
            if (history == null)
            {
                _logger.LogError(
                    "MonitoringWorker: could not resolve ScanHistory for PublicId {HistoryId}.",
                    scanResult.HistoryId);
                return;
            }

            int score = scanResult.Score ?? 0;
            string grade = scanResult.Grade ?? "F";

            // Count findings from the persisted history
            var findings = await unitOfWork.Findings.GetByHistoryIdAsync(history.HistoryId, ct);
            var findingsList = findings.ToList();
            int criticalCount = findingsList.Count(f => f.Severity == Domain.Enums.SeverityLevel.Critical);
            int highCount = findingsList.Count(f => f.Severity == Domain.Enums.SeverityLevel.High);

            // Detect regression and persist snapshot
            await riskDeltaService.DetectAndHandleDeltaAsync(
                monitoredTargetId: target.Id,
                scanHistoryId: history.HistoryId,
                newScore: score,
                newGrade: grade,
                findingsCount: findingsList.Count,
                criticalCount: criticalCount,
                highCount: highCount,
                ct: ct);

            // Update monitoring schedule
            await UpdateTargetScheduleAsync(target, unitOfWork, ct);

            _logger.LogInformation(
                "MonitoringWorker: target {TargetId} processed. Score: {Score} ({Grade}).",
                target.Id, score, grade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MonitoringWorker: failed to process target {TargetId}.", target.Id);
        }
    }

    private async Task UpdateTargetScheduleAsync(
        MonitoredTarget target,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var nextCheck = DateTime.UtcNow.AddDays((int)target.Frequency);
        target.UpdateSchedule(DateTime.UtcNow, nextCheck);

        // Mark as modified so EF Core tracks the change on this untracked entity
        await unitOfWork.MonitoredTargets.UpdateAsync(target, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
