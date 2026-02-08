using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.Helpers;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Commands.User.UpdatePassword;

/// <summary>
/// Handles password update for authenticated users.
/// Verifies current password before allowing update.
/// </summary>
public class UpdatePasswordCommandHandler : ICommandHandler<UpdatePasswordCommand, UpdatePasswordResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePasswordCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdatePasswordResponse> Handle(UpdatePasswordCommand command, CancellationToken ct = default)
    {
        // Validate input
        var validator = new UpdatePasswordCommandValidator();
        var validationResult = await validator.ValidateAsync(command, ct);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new ValidationException(errors);
        }

        // Security check: verify user can only update themselves
        if (command.UserId != command.RequestingUserId)
        {
            throw new ForbiddenException("You can only update your own password");
        }

        // Get user from database
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException($"User with ID {command.UserId} not found");
        }

        // Verify current password
        if (!PasswordUtils.VerifyPassword(command.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Current password is incorrect");
        }

        // Hash new password and update
        var newPasswordHash = PasswordUtils.HashPassword(command.NewPassword);
        user.UpdatePassword(newPasswordHash);

        await _unitOfWork.Users.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Log password change
        await LogPasswordChangeAsync(user.UserId, ct);

        return new UpdatePasswordResponse(
            Success: true,
            Message: "Password updated successfully"
        );
    }

    private async Task LogPasswordChangeAsync(int userId, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_PASSWORD_CHANGED,
            level: "Warning",
            message: "User password changed",
            source: "UpdatePasswordCommandHandler",
            userId: userId
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
