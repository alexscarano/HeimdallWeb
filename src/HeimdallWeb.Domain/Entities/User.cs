using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Exceptions;
using HeimdallWeb.Domain.ValueObjects;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// User entity representing a system user (regular or admin).
/// Supports local authentication and Google OAuth.
/// </summary>
public class User
{
    public int UserId { get; private set; }
    public Guid PublicId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserType UserType { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? ProfileImage { get; private set; }

    // Sprint 5: Google OAuth and password reset fields
    public string AuthProvider { get; private set; } = "Local";
    public string? ExternalId { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetExpires { get; private set; }

    // Navigation properties (collections)
    private readonly List<ScanHistory> _scanHistories = new();
    public IReadOnlyCollection<ScanHistory> ScanHistories => _scanHistories.AsReadOnly();

    private readonly List<UserUsage> _userUsages = new();
    public IReadOnlyCollection<UserUsage> UserUsages => _userUsages.AsReadOnly();

    private readonly List<AuditLog> _auditLogs = new();
    public IReadOnlyCollection<AuditLog> AuditLogs => _auditLogs.AsReadOnly();

    // Parameterless constructor for EF Core
    private User() { }

    /// <summary>
    /// Creates a new local User instance with password hash.
    /// For Google users, use the static factory method CreateGoogleUser().
    /// </summary>
    public User(string username, EmailAddress email, string passwordHash, UserType userType = UserType.Default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username cannot be empty.");

        if (username.Length < 6)
            throw new ValidationException("Username must have at least 6 characters.");

        if (username.Length > 30)
            throw new ValidationException("Username cannot exceed 30 characters.");

        // Password hash is required only for local users; Google users pass string.Empty and call SetGoogleAuth
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ValidationException("Password hash cannot be empty.");

        PublicId = Guid.CreateVersion7();
        Username = username;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash;
        UserType = userType;
        IsActive = true;
        AuthProvider = "Local";
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method for creating Google OAuth users.
    /// Bypasses the constructor's password hash validation.
    /// </summary>
    public static User CreateGoogleUser(string username, EmailAddress email, string googleSubId, string? profileImage = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username cannot be empty.");

        if (username.Length < 2)
            throw new ValidationException("Username must have at least 2 characters.");

        if (username.Length > 30)
            throw new ValidationException("Username cannot exceed 30 characters.");

        if (string.IsNullOrWhiteSpace(googleSubId))
            throw new ValidationException("Google sub ID cannot be empty.");

        var user = new User
        {
            PublicId = Guid.CreateVersion7(),
            Username = username,
            Email = email ?? throw new ArgumentNullException(nameof(email)),
            PasswordHash = string.Empty,
            UserType = UserType.Default,
            IsActive = true,
            AuthProvider = "Google",
            ExternalId = googleSubId,
            ProfileImage = profileImage,
            CreatedAt = DateTime.UtcNow
        };

        return user;
    }

    /// <summary>
    /// Activates the user account.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's active status.
    /// </summary>
    public void UpdateStatus(bool isActive)
    {
        if (IsActive == isActive)
            return;

        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's password hash.
    /// </summary>
    public void UpdatePassword(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ValidationException("Password hash cannot be empty.");

        PasswordHash = hashedPassword;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's profile image.
    /// </summary>
    public void UpdateProfileImage(string? imageUrl)
    {
        ProfileImage = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the username.
    /// </summary>
    public void UpdateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username cannot be empty.");

        if (username.Length < 6 || username.Length > 30)
            throw new ValidationException("Username must be between 6 and 30 characters.");

        Username = username;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the email address.
    /// </summary>
    public void UpdateEmail(EmailAddress email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sprint 5: Configures the user as a Google OAuth user.
    /// Sets AuthProvider to "Google", stores ExternalId (Google sub), and clears PasswordHash.
    /// </summary>
    public void SetGoogleAuth(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ValidationException("Google external ID cannot be empty.");

        AuthProvider = "Google";
        ExternalId = externalId;
        PasswordHash = string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sprint 5: Sets the password reset token (stored as SHA-256 hash) and its expiration time.
    /// </summary>
    public void SetPasswordResetToken(string tokenHash, DateTime expires)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ValidationException("Token hash cannot be empty.");

        PasswordResetToken = tokenHash;
        PasswordResetExpires = expires;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sprint 5: Clears the password reset token and expiration after successful password reset.
    /// </summary>
    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetExpires = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
