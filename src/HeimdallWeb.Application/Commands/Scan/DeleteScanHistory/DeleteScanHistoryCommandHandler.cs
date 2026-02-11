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

        // Get scan history from database by PublicId
        var scanHistory = await _unitOfWork.ScanHistories.GetByPublicIdAsync(request.HistoryId, ct);
        if (scanHistory is null)
        {
            throw new NotFoundException("Scan history", request.HistoryId);
        }

        // SECURITY: 
        // - Regular users can only delete their own scans
        // - Admins can delete any scan
        var requestingUser = await _unitOfWork.Users.GetByPublicIdAsync(request.RequestingUserId, ct);
        if (requestingUser is null)
        {
            throw new UnauthorizedException("Invalid requesting user");
        }

        bool isAdmin = requestingUser.UserType == UserType.Admin;
        bool isOwner = scanHistory.UserId == requestingUser.UserId; // Compare internal IDs

        if (!isAdmin && !isOwner)
        {
            throw new NotFoundException("Scan history", request.HistoryId); // Security: 404 instead of 403
        }

        var target = scanHistory.Target;
        var historyInternalId = scanHistory.HistoryId; // For internal delete operation
        var requestingUserInternalId = requestingUser.UserId; // For logging

        // Delete scan history
        await _unitOfWork.ScanHistories.DeleteAsync(historyInternalId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Log deletion
        await LogScanHistoryDeletionAsync(historyInternalId, requestingUserInternalId, target, ct);

        return new DeleteScanHistoryResponse(
            Success: true,
            HistoryId: request.HistoryId, // Return the PublicId
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
            details: $"History ID: {historyId}, Target: {target}, Deleted by User ID: {userId}",
            userId: userId
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
