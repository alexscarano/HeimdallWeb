using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Auth;
using HeimdallWeb.Application.Helpers;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using HeimdallWeb.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Application.Commands.Auth.GoogleAuth;

/// <summary>
/// Handles Google OAuth authentication flow:
/// 1. Validates the id_token via Google's tokeninfo endpoint.
/// 2. Optionally validates the audience (aud) claim against the configured ClientId.
/// 3. Finds existing user by Google sub or creates a new Google user.
/// 4. Verifies the user is active.
/// 5. Generates and returns a JWT token (same DTO as the login endpoint).
/// </summary>
public class GoogleAuthCommandHandler : ICommandHandler<GoogleAuthCommand, LoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleAuthCommandHandler> _logger;

    private const string GoogleTokenInfoUrl = "https://oauth2.googleapis.com/tokeninfo?id_token=";

    public GoogleAuthCommandHandler(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleAuthCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginResponse> Handle(GoogleAuthCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.IdToken))
            throw new UnauthorizedException("Google ID token is required.");

        // Step 1: Validate id_token via Google's official tokeninfo endpoint
        var tokenInfo = await ValidateGoogleTokenAsync(command.IdToken, ct);

        // Step 2: Extract required claims from token info
        var googleSubId = tokenInfo.Sub;
        var email = tokenInfo.Email;
        var name = tokenInfo.Name ?? email.Split('@')[0];
        var picture = tokenInfo.Picture;

        if (string.IsNullOrWhiteSpace(googleSubId) || string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Google token missing sub or email claims from IP {RemoteIp}", command.RemoteIp);
            throw new UnauthorizedException("Invalid Google token: missing required claims.");
        }

        // Step 3: Validate audience — MANDATORY when Google:ClientId is configured
        // Security: if ClientId is not configured, reject all Google auth attempts to prevent token forgery
        var configuredClientId = _configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(configuredClientId))
        {
            _logger.LogError("Google:ClientId is not configured. Rejecting Google OAuth attempt from IP {RemoteIp}", command.RemoteIp);
            throw new UnauthorizedException("Google authentication is not available at this time.");
        }

        if (tokenInfo.Aud != configuredClientId)
        {
            _logger.LogWarning(
                "Google token audience mismatch. Expected {Expected}, got {Actual} from IP {RemoteIp}",
                configuredClientId,
                tokenInfo.Aud,
                command.RemoteIp);
            throw new UnauthorizedException("Invalid Google token: audience mismatch.");
        }

        // Step 3b: Verify the Google account email is verified
        // Security: prevents using unverified Google accounts to hijack existing local accounts
        if (tokenInfo.EmailVerified != "true")
        {
            _logger.LogWarning(
                "Google token email not verified for {Email} from IP {RemoteIp}",
                email,
                command.RemoteIp);
            throw new UnauthorizedException("Google account email must be verified.");
        }

        // Step 4: Create EmailAddress value object
        EmailAddress emailAddress;
        try
        {
            emailAddress = EmailAddress.Create(email.ToLower().Trim());
        }
        catch
        {
            throw new UnauthorizedException("Invalid Google token: email address is not valid.");
        }

        // Step 5: Find existing user by Google sub or email
        var user = await _unitOfWork.Users.GetByExternalIdAsync(googleSubId, ct);

        if (user is not null)
        {
            // Existing Google user — update profile image if changed
            if (user.ProfileImage != picture)
            {
                user.UpdateProfileImage(picture);
                await _unitOfWork.Users.UpdateAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }
        else
        {
            // Check if a local account exists with this email
            var existingLocalUser = await _unitOfWork.Users.GetByEmailAsync(emailAddress, ct);
            if (existingLocalUser is not null)
            {
                // Security: DO NOT auto-link local accounts to Google without explicit user consent.
                // Auto-linking is an account takeover vector: an attacker who controls a Google account
                // with the same email could hijack the local account and erase its password.
                // The user must log in with their local credentials first, then link Google in profile settings.
                _logger.LogWarning(
                    "Google OAuth attempted for existing local account {Email} from IP {RemoteIp}. Auto-link denied.",
                    email,
                    command.RemoteIp);
                throw new UnauthorizedException(
                    "An account with this email already exists. Please log in with your password and link Google from your profile settings.");
            }
            else
            {
                // New user — create a Google OAuth account
                // Generate a unique username from the display name
                var baseUsername = SanitizeUsername(name);
                var username = await EnsureUniqueUsernameAsync(baseUsername, ct);

                user = Domain.Entities.User.CreateGoogleUser(
                    username: username,
                    email: emailAddress,
                    googleSubId: googleSubId,
                    profileImage: picture);

                await _unitOfWork.Users.AddAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "New Google user created: {Username} ({Email}) from IP {RemoteIp}",
                    username,
                    email,
                    command.RemoteIp);
            }
        }

        // Step 6: Verify user is active
        if (!user.IsActive)
        {
            await LogGoogleAuthFailedAsync(user.UserId, command.RemoteIp, "Account blocked", ct);
            throw new ForbiddenException("Your account is blocked. Contact administrator.");
        }

        // Step 7: Generate JWT token
        var token = TokenService.GenerateToken(user, _configuration);

        // Step 8: Audit log
        await LogGoogleAuthSuccessAsync(user.UserId, command.RemoteIp, ct);

        return new LoginResponse(
            UserId: user.PublicId,
            Username: user.Username,
            Email: user.Email.Value,
            UserType: (int)user.UserType,
            Token: token,
            IsActive: user.IsActive
        );
    }

    /// <summary>
    /// Calls Google's tokeninfo endpoint and deserializes the response.
    /// Throws UnauthorizedException if the token is invalid.
    /// </summary>
    private async Task<GoogleTokenInfo> ValidateGoogleTokenAsync(string idToken, CancellationToken ct)
    {
        using var httpClient = _httpClientFactory.CreateClient("Google");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"{GoogleTokenInfoUrl}{Uri.EscapeDataString(idToken)}", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach Google tokeninfo endpoint");
            throw new UnauthorizedException("Unable to validate Google token. Please try again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Google tokeninfo returned status {Status} for provided id_token",
                (int)response.StatusCode);
            throw new UnauthorizedException("Invalid Google token.");
        }

        var tokenInfo = await response.Content.ReadFromJsonAsync<GoogleTokenInfo>(cancellationToken: ct);
        if (tokenInfo is null)
            throw new UnauthorizedException("Invalid Google token: empty response.");

        return tokenInfo;
    }

    /// <summary>
    /// Ensures the username is URL-safe, within length limits, and contains at least 6 characters.
    /// </summary>
    private static string SanitizeUsername(string name)
    {
        // Keep only alphanumerics and underscores
        var sanitized = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        if (sanitized.Length < 6) sanitized = sanitized.PadRight(6, '0');
        if (sanitized.Length > 28) sanitized = sanitized[..28];
        return sanitized;
    }

    /// <summary>
    /// Appends a numeric suffix to the username until a unique one is found.
    /// </summary>
    private async Task<string> EnsureUniqueUsernameAsync(string baseUsername, CancellationToken ct)
    {
        var candidate = baseUsername;
        var suffix = 1;

        while (await _unitOfWork.Users.ExistsByUsernameAsync(candidate, ct))
        {
            candidate = $"{baseUsername}{suffix}";
            if (candidate.Length > 30) candidate = $"{baseUsername[..(30 - suffix.ToString().Length)]}{suffix}";
            suffix++;
        }

        return candidate;
    }

    private async Task LogGoogleAuthSuccessAsync(int userId, string remoteIp, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.GOOGLE_AUTH_SUCCESS,
            level: "Info",
            message: "Google OAuth authentication successful",
            source: "GoogleAuthCommandHandler",
            userId: userId,
            remoteIp: remoteIp
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task LogGoogleAuthFailedAsync(int userId, string remoteIp, string reason, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.GOOGLE_AUTH_FAILED,
            level: "Warning",
            message: "Google OAuth authentication failed",
            source: "GoogleAuthCommandHandler",
            details: $"Reason: {reason}",
            userId: userId,
            remoteIp: remoteIp
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>
    /// DTO for deserializing the Google tokeninfo API response.
    /// Only maps the fields needed by this handler.
    /// </summary>
    private sealed class GoogleTokenInfo
    {
        [JsonPropertyName("sub")]
        public string Sub { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("aud")]
        public string? Aud { get; set; }

        [JsonPropertyName("email_verified")]
        public string? EmailVerified { get; set; }
    }
}
