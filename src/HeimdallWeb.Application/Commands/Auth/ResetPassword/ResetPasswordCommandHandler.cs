using System.Text;
using System.Security.Cryptography;
using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.Helpers;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Application.Commands.Auth.ResetPassword;

/// <summary>
/// Handles the password reset flow:
/// 1. Hashes the incoming raw token (same SHA-256 algorithm used during ForgotPassword).
/// 2. Looks up the user by the stored token hash.
/// 3. Validates token existence and expiry.
/// 4. Validates the new password strength.
/// 5. Updates the password hash and clears the reset token.
/// </summary>
public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new Common.Exceptions.ValidationException("Token", "Token is required.");

        if (string.IsNullOrWhiteSpace(command.NewPassword))
            throw new Common.Exceptions.ValidationException("NewPassword", "New password is required.");

        // Hash the received raw token to compare against DB
        var tokenHash = ComputeSha256Hash(command.Token.Trim());

        // Lookup user by token hash (with tracking for update)
        var user = await _unitOfWork.Users.GetByPasswordResetTokenAsync(tokenHash, ct);
        if (user is null)
        {
            _logger.LogWarning(
                "Password reset attempted with invalid token from IP {RemoteIp}",
                command.RemoteIp);
            throw new Common.Exceptions.ValidationException("Token", "Invalid or expired token.");
        }

        // Validate token expiry
        if (user.PasswordResetExpires is null || user.PasswordResetExpires < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Password reset attempted with expired token for user {UserId} from IP {RemoteIp}",
                user.UserId,
                command.RemoteIp);
            throw new Common.Exceptions.ValidationException("Token", "Token has expired. Please request a new password reset.");
        }

        // Validate new password strength
        ValidatePasswordStrength(command.NewPassword);

        // Update password hash and clear the reset token
        var newPasswordHash = PasswordUtils.HashPassword(command.NewPassword);
        user.UpdatePassword(newPasswordHash);
        user.ClearPasswordResetToken();

        await _unitOfWork.Users.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Audit log
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.PASSWORD_RESET_COMPLETED,
            level: "Info",
            message: "Password reset completed successfully",
            source: "ResetPasswordCommandHandler",
            userId: user.UserId,
            remoteIp: command.RemoteIp
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Password reset completed for user {UserId} from IP {RemoteIp}",
            user.UserId,
            command.RemoteIp);

        return new ResetPasswordResponse("Password updated successfully.");
    }

    /// <summary>
    /// Validates that the new password meets minimum security requirements:
    /// at least 8 characters, 1 uppercase letter, and 1 digit.
    /// </summary>
    private static void ValidatePasswordStrength(string password)
    {
        if (password.Length < 8)
            throw new Common.Exceptions.ValidationException("NewPassword", "Password must be at least 8 characters long.");

        if (!password.Any(char.IsUpper))
            throw new Common.Exceptions.ValidationException("NewPassword", "Password must contain at least one uppercase letter.");

        if (!password.Any(char.IsDigit))
            throw new Common.Exceptions.ValidationException("NewPassword", "Password must contain at least one number.");
    }

    /// <summary>
    /// Computes SHA-256 hash of the raw token, returning a lowercase hex string.
    /// Must match the algorithm used in ForgotPasswordCommandHandler.
    /// </summary>
    private static string ComputeSha256Hash(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
