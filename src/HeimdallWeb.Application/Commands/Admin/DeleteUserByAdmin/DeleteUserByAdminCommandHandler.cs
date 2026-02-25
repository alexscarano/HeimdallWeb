using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Admin;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Commands.Admin.DeleteUserByAdmin;

/// <summary>
/// Handles user deletion by administrators.
/// Implements strict security and business rules.
/// </summary>
public class DeleteUserByAdminCommandHandler : ICommandHandler<DeleteUserByAdminCommand, DeleteUserByAdminResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserByAdminCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteUserByAdminResponse> Handle(DeleteUserByAdminCommand request, CancellationToken ct = default)
    {
        // SECURITY: Verify requesting user is Admin (must be first check)
        if (request.RequestingUserType != UserType.Admin)
        {
            throw new ForbiddenException("Only administrators can delete users");
        }

        // Validate input
        var validator = new DeleteUserByAdminCommandValidator();
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

        // BUSINESS RULE: Cannot delete yourself (already validated above, redundant check)
        if (request.UserId == request.RequestingUserId)
        {
            throw new ValidationException("UserId", "Cannot delete yourself");
        }

        // Resolve requesting user to get internal ID for logging
        var requestingUser = await _unitOfWork.Users.GetByPublicIdAsync(request.RequestingUserId, ct);
        if (requestingUser is null)
        {
            throw new UnauthorizedException("Invalid requesting user");
        }

        // Get target user from database by PublicId
        var user = await _unitOfWork.Users.GetByPublicIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // BUSINESS RULE: Cannot delete admin users
        if (user.UserType == UserType.Admin)
        {
            throw new ValidationException("UserType", "Cannot delete admin users");
        }

        var deletedUsername = user.Username;
        var deletedUserPublicId = user.PublicId;
        var deletedUserInternalId = user.UserId; // Keep for logging
        var requestingUserInternalId = requestingUser.UserId; // For logging

        // Delete user
        await _unitOfWork.Users.DeleteAsync(user.UserId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Log deletion
        await LogUserDeletionByAdminAsync(deletedUserInternalId, deletedUsername, requestingUserInternalId, ct);

        return new DeleteUserByAdminResponse(
            Success: true,
            DeletedUserId: deletedUserPublicId,
            DeletedUsername: deletedUsername
        );
    }

    private async Task LogUserDeletionByAdminAsync(int deletedUserId, string deletedUsername, int adminUserId, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_DELETED_BY_ADMIN,
            level: "Warning",
            message: "User deleted by administrator",
            source: "DeleteUserByAdminCommandHandler",
            details: $"Admin (ID: {adminUserId}) deleted user '{deletedUsername}' (ID: {deletedUserId})",
            userId: adminUserId
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
