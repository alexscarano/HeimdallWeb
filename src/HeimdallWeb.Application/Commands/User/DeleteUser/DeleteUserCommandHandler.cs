using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Application.Helpers;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Commands.User.DeleteUser;

/// <summary>
/// Handles user account deletion with password verification.
/// Requires explicit confirmation and password validation for security.
/// </summary>
public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, DeleteUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteUserResponse> Handle(DeleteUserCommand request, CancellationToken ct = default)
    {
        // Validate input
        var validator = new DeleteUserCommandValidator();
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

        // Security check: 
        // - Admins can delete regular users (UserType = 1) but NOT other admins
        // - Regular users can only delete themselves (with password)
        var requestingUser = await _unitOfWork.Users.GetByPublicIdAsync(request.RequestingUserId, ct);
        if (requestingUser is null)
        {
            throw new UnauthorizedException("Invalid requesting user");
        }

        // Get target user to check their type
        var targetUser = await _unitOfWork.Users.GetByPublicIdAsync(request.UserId, ct);
        if (targetUser is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        bool isAdmin = requestingUser.UserType == UserType.Admin;
        bool isDeletingSelf = request.UserId == request.RequestingUserId;
        bool targetIsAdmin = targetUser.UserType == UserType.Admin;

        if (!isAdmin && !isDeletingSelf)
        {
            throw new ForbiddenException("You can only delete your own account");
        }

        // Admin cannot delete other admins
        if (isAdmin && targetIsAdmin && !isDeletingSelf)
        {
            throw new ForbiddenException("Admins cannot delete other admin accounts");
        }

        // Verify password (only required if user is deleting their own account)
        if (isDeletingSelf && !PasswordUtils.VerifyPassword(request.Password, targetUser.PasswordHash))
        {
            throw new UnauthorizedException("Invalid password. Account deletion cancelled.");
        }

        // Delete user
        await _unitOfWork.Users.DeleteAsync(targetUser.UserId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Log account deletion
        await LogAccountDeletionAsync(targetUser.UserId, targetUser.Username, requestingUser.UserId, isAdmin, ct);

        return new DeleteUserResponse(
            Message: isAdmin && !isDeletingSelf 
                ? $"User '{targetUser.Username}' deleted successfully by admin"
                : "Account deleted successfully",
            UserId: targetUser.PublicId
        );
    }

    private async Task LogAccountDeletionAsync(int userId, string username, int requestingUserId, bool deletedByAdmin, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_ACCOUNT_DELETED,
            level: "Warning",
            message: deletedByAdmin ? "User account deleted by admin" : "User account self-deleted",
            source: "DeleteUserCommandHandler",
            details: deletedByAdmin 
                ? $"User '{username}' (ID: {userId}) was deleted by admin (ID: {requestingUserId})"
                : $"User '{username}' (ID: {userId}) deleted their own account",
            userId: requestingUserId
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
