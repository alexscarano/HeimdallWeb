using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// Technology entity representing a detected technology during a scan.
/// </summary>
public class Technology
{
    public int TechnologyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Version { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public int? HistoryId { get; private set; }

    // Navigation property (parent)
    public ScanHistory? History { get; private set; }

    // Parameterless constructor for EF Core
    private Technology() { }

    /// <summary>
    /// Creates a new Technology instance.
    /// </summary>
    public Technology(
        string name,
        string category,
        string description,
        string? version = null,
        int? historyId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Technology name cannot be empty.");

        if (name.Length > 100)
            throw new ValidationException("Technology name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(category))
            throw new ValidationException("Technology category cannot be empty.");

        if (category.Length > 50)
            throw new ValidationException("Technology category cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Technology description cannot be empty.");

        if (description.Length > 1000)
            throw new ValidationException("Technology description cannot exceed 1000 characters.");

        if (version?.Length > 30)
            throw new ValidationException("Technology version cannot exceed 30 characters.");

        Name = name;
        Version = version;
        Category = category;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        HistoryId = historyId;
    }
}
