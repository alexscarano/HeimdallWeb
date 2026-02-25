using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Auth;
using HeimdallWeb.Application.Helpers;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using HeimdallWeb.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace HeimdallWeb.Application.Commands.Auth.Register;

/// <summary>
/// Handles user registration with validation, duplicate checking, and JWT token generation.
/// </summary>
public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken ct = default)
    {
        // Validate input
        var validator = new RegisterUserCommandValidator();
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

        // Normalize inputs
        var emailInput = request.Email.ToLower().Trim();
        var usernameInput = request.Username.Trim();

        // Create EmailAddress value object (validates format)
        EmailAddress email;
        try
        {
            email = EmailAddress.Create(emailInput);
        }
        catch (Domain.Exceptions.ValidationException ex)
        {
            await LogFailedRegistrationAsync(emailInput, usernameInput, request.RemoteIp, "Invalid email format", ct);
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Email", new[] { ex.Message } }
            });
        }

        // Check if email already exists
        var emailExists = await _unitOfWork.Users.ExistsByEmailAsync(email, ct);
        if (emailExists)
        {
            await LogFailedRegistrationAsync(emailInput, usernameInput, request.RemoteIp, "Email already registered", ct);
            throw new ConflictException("An account with this email already exists");
        }

        // Check if username already exists
        var usernameExists = await _unitOfWork.Users.ExistsByUsernameAsync(usernameInput, ct);
        if (usernameExists)
        {
            await LogFailedRegistrationAsync(emailInput, usernameInput, request.RemoteIp, "Username already taken", ct);
            throw new ConflictException("This username is already taken");
        }

        // Hash password using PBKDF2
        var passwordHash = PasswordUtils.HashPassword(request.Password);

        // Create new User entity (UserType.Default = Regular user, IsActive = true by default)
        Domain.Entities.User newUser;
        try
        {
            newUser = new Domain.Entities.User(
                username: usernameInput,
                email: email,
                passwordHash: passwordHash,
                userType: UserType.Default
            );
        }
        catch (Domain.Exceptions.ValidationException ex)
        {
            await LogFailedRegistrationAsync(emailInput, usernameInput, request.RemoteIp, $"Entity validation failed: {ex.Message}", ct);
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "User", new[] { ex.Message } }
            });
        }

        // Add user to repository
        var createdUser = await _unitOfWork.Users.AddAsync(newUser, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Generate JWT token
        var token = TokenService.GenerateToken(createdUser, _configuration);

        // Log successful registration
        await LogSuccessfulRegistrationAsync(createdUser.UserId, usernameInput, emailInput, request.RemoteIp, ct);

        return new RegisterUserResponse(
            UserId: createdUser.PublicId,
            Username: createdUser.Username,
            Email: createdUser.Email.Value,
            UserType: (int)createdUser.UserType,
            Token: token,
            IsActive: createdUser.IsActive
        );
    }

    /// <summary>
    /// Logs successful user registration event.
    /// </summary>
    private async Task LogSuccessfulRegistrationAsync(
        int userId,
        string username,
        string email,
        string remoteIp,
        CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_REGISTERED,
            level: "Info",
            message: "New user registered successfully",
            source: "RegisterUserCommandHandler",
            details: $"Username: {username}, Email: {email}",
            userId: userId,
            remoteIp: remoteIp
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Logs failed registration attempt with reason.
    /// </summary>
    private async Task LogFailedRegistrationAsync(
        string email,
        string username,
        string remoteIp,
        string reason,
        CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_REGISTRATION_FAILED,
            level: "Warning",
            message: "Failed registration attempt",
            source: "RegisterUserCommandHandler",
            details: $"Email: {email}, Username: {username}. Reason: {reason}",
            remoteIp: remoteIp
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
