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

        // Security check: verify user can only delete themselves
        if (request.UserId != request.RequestingUserId)
        {
            throw new ForbiddenException("You can only delete your own account");
        }

        // Get user from database
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Verify password
        if (!PasswordUtils.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid password. Account deletion cancelled.");
        }

        // Delete user
        await _unitOfWork.Users.DeleteAsync(user.UserId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Log account deletion
        await LogAccountDeletionAsync(user.UserId, user.Username, ct);

        return new DeleteUserResponse(
            Message: "Account deleted successfully",
            UserId: user.UserId
        );
    }

    private async Task LogAccountDeletionAsync(int userId, string username, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_ACCOUNT_DELETED,
            level: "Warning",
            message: "User account deleted",
            source: "DeleteUserCommandHandler",
            details: $"User '{username}' (ID: {userId}) deleted their account",
            userId: userId
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
