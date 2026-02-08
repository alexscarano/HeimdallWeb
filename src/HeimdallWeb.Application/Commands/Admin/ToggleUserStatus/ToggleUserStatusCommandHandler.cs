using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Admin;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Commands.Admin.ToggleUserStatus;

/// <summary>
/// Handles toggling user account status (activate/deactivate).
/// Admin-only operation with strict business rules.
/// </summary>
public class ToggleUserStatusCommandHandler : ICommandHandler<ToggleUserStatusCommand, ToggleUserStatusResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public ToggleUserStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ToggleUserStatusResponse> Handle(ToggleUserStatusCommand request, CancellationToken ct = default)
    {
        // Validate input
        var validator = new ToggleUserStatusCommandValidator();
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

        // SECURITY: Verify requesting user is Admin
        if (request.RequestingUserType != UserType.Admin)
        {
            throw new ForbiddenException("Only administrators can toggle user status");
        }

        // Get target user from database by PublicId
        var user = await _unitOfWork.Users.GetByPublicIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // BUSINESS RULE: Cannot toggle admin users
        if (user.UserType == UserType.Admin)
        {
            throw new ValidationException("UserType", "Cannot block/unblock admin users");
        }

        var oldStatus = user.IsActive;

        // Update user status using domain method
        user.UpdateStatus(request.IsActive);

        await _unitOfWork.SaveChangesAsync(ct);

        // Log status toggle
        await LogUserStatusToggleAsync(user.UserId, user.Username, oldStatus, request.IsActive, ct);

        return new ToggleUserStatusResponse(
            UserId: user.PublicId,
            Username: user.Username,
            IsActive: user.IsActive
        );
    }

    private async Task LogUserStatusToggleAsync(int userId, string username, bool oldStatus, bool newStatus, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_STATUS_TOGGLED,
            level: "Warning",
            message: "User status toggled by admin",
            source: "ToggleUserStatusCommandHandler",
            details: $"User '{username}' (ID: {userId}) status changed from {oldStatus} to {newStatus}",
            userId: userId
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
