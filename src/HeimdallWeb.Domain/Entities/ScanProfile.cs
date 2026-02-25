namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// Represents a named scan profile that groups scanner selection and timeout settings.
/// System profiles (IsSystem=true) ship as seed data and cannot be deleted.
/// Custom profiles can be created by users to save preferred scanner configurations.
/// Stored in tb_scan_profile.
/// </summary>
public class ScanProfile
{
    public int Id { get; private set; }

    /// <summary>
    /// Human-readable name for this profile, e.g. "Quick", "Standard", "Deep".
    /// Unique across all profiles.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description explaining when to use this profile.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// JSON payload describing which scanners to run and the scan timeout.
    /// Example: {"scanners":["Headers","SSL","Redirect"],"timeoutSeconds":30}
    /// </summary>
    public string ConfigJson { get; private set; } = string.Empty;

    /// <summary>
    /// When true this profile was created by the system seed and must not be deleted.
    /// </summary>
    public bool IsSystem { get; private set; }

    // Parameterless constructor for EF Core
    private ScanProfile() { }

    public ScanProfile(string name, string description, string configJson, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (name.Length > 50)
            throw new ArgumentOutOfRangeException(nameof(name), "Name cannot exceed 50 characters.");

        if (description.Length > 200)
            throw new ArgumentOutOfRangeException(nameof(description), "Description cannot exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(configJson))
            throw new ArgumentException("ConfigJson cannot be empty.", nameof(configJson));

        Name = name;
        Description = description;
        ConfigJson = configJson;
        IsSystem = isSystem;
    }

    /// <summary>
    /// Updates the profile name and description.
    /// System profiles can still have their description updated.
    /// </summary>
    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (name.Length > 50)
            throw new ArgumentOutOfRangeException(nameof(name), "Name cannot exceed 50 characters.");

        if (description.Length > 200)
            throw new ArgumentOutOfRangeException(nameof(description), "Description cannot exceed 200 characters.");

        Name = name;
        Description = description;
    }

    /// <summary>
    /// Replaces the JSON configuration for scanners and timeout.
    /// </summary>
    public void UpdateConfig(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            throw new ArgumentException("ConfigJson cannot be empty.", nameof(configJson));

        ConfigJson = configJson;
    }
}
