using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Exceptions;
using HeimdallWeb.Domain.ValueObjects;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// User entity representing a system user (regular or admin).
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
    /// Creates a new User instance.
    /// </summary>
    public User(string username, EmailAddress email, string passwordHash, UserType userType = UserType.Default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Username cannot be empty.");

        if (username.Length < 6)
            throw new ValidationException("Username must have at least 6 characters.");

        if (username.Length > 30)
            throw new ValidationException("Username cannot exceed 30 characters.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ValidationException("Password hash cannot be empty.");

        PublicId = Guid.CreateVersion7();
        Username = username;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash;
        UserType = userType;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
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
}
