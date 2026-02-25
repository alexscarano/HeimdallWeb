using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using HeimdallWeb.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Application.Commands.Auth.ForgotPassword;

/// <summary>
/// Handles the forgot-password flow:
/// 1. Looks up the user by email — if not found, returns the same neutral response (no enumeration).
/// 2. Generates a cryptographically secure raw token (32 random bytes, Base64 URL-safe).
/// 3. Stores only the SHA-256 hash of that token in the database (never the raw token).
/// 4. Sets a 1-hour expiry window.
/// 5. Sends the reset link containing the raw token via email.
/// </summary>
public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly IValidator<ForgotPasswordCommand> _validator;

    // Security: always return this message regardless of whether the email exists
    private const string NeutralMessage = "If this email is registered, a reset link was sent.";

    public ForgotPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger,
        IValidator<ForgotPasswordCommand> validator)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand command, CancellationToken ct = default)
    {
        // Validate input format (throws ValidationException on invalid email)
        await _validator.ValidateAndThrowAsync(command, ct);

        var emailInput = command.Email.Trim().ToLower();

        // Attempt to create a valid EmailAddress value object
        EmailAddress emailAddress;
        try
        {
            emailAddress = EmailAddress.Create(emailInput);
        }
        catch
        {
            // Invalid email format — return neutral response without revealing anything
            _logger.LogWarning("Forgot-password requested with invalid email format from IP {RemoteIp}", command.RemoteIp);
            return new ForgotPasswordResponse(NeutralMessage);
        }

        // Lookup user — always return neutral response if not found (prevents enumeration)
        var user = await _unitOfWork.Users.GetByEmailAsync(emailAddress, ct);
        if (user is null)
        {
            _logger.LogInformation(
                "Forgot-password requested for non-existent email {Email} from IP {RemoteIp}",
                emailInput,
                command.RemoteIp);
            return new ForgotPasswordResponse(NeutralMessage);
        }

        // Generate a cryptographically secure raw token (256 bits of entropy)
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        // Store only the SHA-256 hash; the raw token travels over email only
        var tokenHash = ComputeSha256Hash(rawToken);

        // Set expiry to 1 hour from now (UTC)
        var expires = DateTime.UtcNow.AddHours(1);
        user.SetPasswordResetToken(tokenHash, expires);

        await _unitOfWork.Users.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Build reset link using the raw (unhashed) token
        // Security: use configured frontend base URL — MUST be HTTPS in production
        var frontendBaseUrl = _configuration["FrontendBaseUrl"] ?? "http://localhost:3000";
        var resetLink = $"{frontendBaseUrl}/auth/reset-password?token={Uri.EscapeDataString(rawToken)}";

        // Send email — graceful degradation: logs and skips if SMTP not configured
        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                toEmail: user.Email.Value,
                toName: user.Username,
                resetLink: resetLink,
                ct: ct);
        }
        catch (Exception ex)
        {
            // Email delivery failure should not block the flow; link is already saved in DB
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email.Value);
        }

        // Audit log
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.PASSWORD_RESET_REQUESTED,
            level: "Info",
            message: "Password reset requested",
            source: "ForgotPasswordCommandHandler",
            details: $"Email: {user.Email.Value}",
            userId: user.UserId,
            remoteIp: command.RemoteIp
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new ForgotPasswordResponse(NeutralMessage);
    }

    /// <summary>
    /// Computes the SHA-256 hash of the raw token and returns it as a lowercase hex string.
    /// </summary>
    private static string ComputeSha256Hash(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
