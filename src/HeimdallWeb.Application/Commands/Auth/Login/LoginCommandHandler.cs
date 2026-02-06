using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Auth;
using HeimdallWeb.Application.Helpers;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using HeimdallWeb.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace HeimdallWeb.Application.Commands.Auth.Login;

/// <summary>
/// Handles user authentication (login) with email/username and password.
/// Returns JWT token on success.
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken ct = default)
    {
        // Validate input
        var validator = new LoginCommandValidator();
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

        // Try to find user by email first
        var emailOrUsername = request.EmailOrUsername.ToLower().Trim();
        Domain.Entities.User? user = null;

        // Check if input looks like an email
        if (emailOrUsername.Contains('@'))
        {
            var emailResult = EmailAddress.Create(emailOrUsername);
            user = await _unitOfWork.Users.GetByEmailAsync(emailResult, ct);
        }

        // If not found by email or input was username, search by username
        if (user is null)
        {
            user = await _unitOfWork.Users.GetByUsernameAsync(emailOrUsername, ct);
        }

        // User not found
        if (user is null)
        {
            // Log failed login attempt
            await LogFailedLoginAsync(emailOrUsername, request.RemoteIp, "User not found", ct);
            throw new UnauthorizedException("Invalid credentials");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            await LogFailedLoginAsync(emailOrUsername, request.RemoteIp, "Account is blocked", ct);
            throw new ForbiddenException("Your account is blocked. Contact administrator.");
        }

        // Verify password
        if (!PasswordUtils.VerifyPassword(request.Password, user.PasswordHash))
        {
            await LogFailedLoginAsync(emailOrUsername, request.RemoteIp, "Invalid password", ct);
            throw new UnauthorizedException("Invalid credentials");
        }

        // Generate JWT token
        var token = TokenService.GenerateToken(user, _configuration);

        // Log successful login
        await LogSuccessfulLoginAsync(user.UserId, request.RemoteIp, ct);

        return new LoginResponse(
            UserId: user.UserId,
            Username: user.Username,
            Email: user.Email.Value,
            UserType: (int)user.UserType,
            Token: token,
            IsActive: user.IsActive
        );
    }

    private async Task LogSuccessfulLoginAsync(int userId, string remoteIp, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_LOGIN,
            level: "Info",
            message: "User authenticated successfully",
            source: "LoginCommandHandler",
            userId: userId,
            remoteIp: remoteIp
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogFailedLoginAsync(string emailOrUsername, string remoteIp, string reason, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_LOGIN_FAILED,
            level: "Warning",
            message: "Failed authentication attempt",
            source: "LoginCommandHandler",
            details: $"Attempted login with: {emailOrUsername}. Reason: {reason}",
            remoteIp: remoteIp
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
