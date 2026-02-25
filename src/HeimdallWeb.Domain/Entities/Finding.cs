using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// Finding entity representing a security vulnerability or issue discovered during a scan.
/// </summary>
public class Finding
{
    public int FindingId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public SeverityLevel Severity { get; private set; }
    public string Evidence { get; private set; } = string.Empty;
    public string Recommendation { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public int? HistoryId { get; private set; }

    // Navigation property (parent)
    public ScanHistory? History { get; private set; }

    // Parameterless constructor for EF Core
    private Finding() { }

    /// <summary>
    /// Creates a new Finding instance.
    /// </summary>
    public Finding(
        string type,
        string description,
        SeverityLevel severity,
        string evidence,
        string recommendation,
        int? historyId = null)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ValidationException("Finding type cannot be empty.");

        if (type.Length > 100)
            throw new ValidationException("Finding type cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Finding description cannot be empty.");

        if (string.IsNullOrWhiteSpace(evidence))
            throw new ValidationException("Finding evidence cannot be empty.");

        Type = type;
        Description = description;
        Severity = severity;
        Evidence = evidence;
        Recommendation = recommendation ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        HistoryId = historyId;
    }

    /// <summary>
    /// Updates the severity level of the finding.
    /// </summary>
    public void UpdateSeverity(SeverityLevel newSeverity)
    {
        Severity = newSeverity;
    }

    /// <summary>
    /// Updates the recommendation for the finding.
    /// </summary>
    public void UpdateRecommendation(string recommendation)
    {
        if (recommendation.Length > 255)
            throw new ValidationException("Recommendation cannot exceed 255 characters.");

        Recommendation = recommendation ?? string.Empty;
    }
}
