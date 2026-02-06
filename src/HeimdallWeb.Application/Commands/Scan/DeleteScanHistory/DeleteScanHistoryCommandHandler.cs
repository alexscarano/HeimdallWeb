using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Commands.Scan.DeleteScanHistory;

/// <summary>
/// Handles deletion of scan history records.
/// Implements security check to ensure users can only delete their own scans.
/// </summary>
public class DeleteScanHistoryCommandHandler : ICommandHandler<DeleteScanHistoryCommand, DeleteScanHistoryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteScanHistoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteScanHistoryResponse> Handle(DeleteScanHistoryCommand request, CancellationToken ct = default)
    {
        // Validate input
        var validator = new DeleteScanHistoryCommandValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            throw new ValidationException(errors);
        }

        // Get scan history from database
        var scanHistory = await _unitOfWork.ScanHistories.GetByIdAsync(request.HistoryId, ct);
        if (scanHistory is null)
        {
            throw new NotFoundException($"Scan history with ID {request.HistoryId} not found");
        }

        // SECURITY: Verify ownership - users can only delete their own scans
        if (scanHistory.UserId != request.RequestingUserId)
        {
            throw new ForbiddenException("You can only delete your own scan history");
        }

        var target = scanHistory.Target;

        // Delete scan history
        await _unitOfWork.ScanHistories.DeleteAsync(request.HistoryId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Log deletion
        await LogScanHistoryDeletionAsync(request.HistoryId, request.RequestingUserId, target, ct);

        return new DeleteScanHistoryResponse(
            Success: true,
            HistoryId: request.HistoryId,
            Target: target
        );
    }

    private async Task LogScanHistoryDeletionAsync(int historyId, int userId, string target, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.SCAN_HISTORY_DELETED,
            level: "Info",
            message: "Scan history deleted",
            source: "DeleteScanHistoryCommandHandler",
            details: $"History ID: {historyId}, Target: {target}",
            userId: userId
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
