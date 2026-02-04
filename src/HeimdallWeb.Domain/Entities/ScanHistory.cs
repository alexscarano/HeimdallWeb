using HeimdallWeb.Domain.Exceptions;
using HeimdallWeb.Domain.ValueObjects;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// ScanHistory entity representing a security scan session.
/// </summary>
public class ScanHistory
{
    public int HistoryId { get; private set; }
    public ScanTarget Target { get; private set; } = null!;
    public string RawJsonResult { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public bool HasCompleted { get; private set; }
    public ScanDuration? Duration { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public int UserId { get; private set; }

    // Navigation properties (collections)
    private readonly List<Finding> _findings = new();
    public IReadOnlyCollection<Finding> Findings => _findings.AsReadOnly();

    private readonly List<Technology> _technologies = new();
    public IReadOnlyCollection<Technology> Technologies => _technologies.AsReadOnly();

    private readonly List<IASummary> _iaSummaries = new();
    public IReadOnlyCollection<IASummary> IASummaries => _iaSummaries.AsReadOnly();

    private readonly List<AuditLog> _auditLogs = new();
    public IReadOnlyCollection<AuditLog> AuditLogs => _auditLogs.AsReadOnly();

    // Navigation property (parent)
    public User? User { get; private set; }

    // Parameterless constructor for EF Core
    private ScanHistory() { }

    /// <summary>
    /// Creates a new ScanHistory instance.
    /// </summary>
    public ScanHistory(ScanTarget target, int userId)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        UserId = userId;
        HasCompleted = false;
        CreatedDate = DateTime.UtcNow;
        RawJsonResult = "{}";
        Summary = string.Empty;
    }

    /// <summary>
    /// Marks the scan as completed and sets the duration.
    /// </summary>
    public void CompleteScan(TimeSpan duration, string rawJsonResult, string summary)
    {
        if (HasCompleted)
            throw new ValidationException("Scan is already marked as completed.");

        if (string.IsNullOrWhiteSpace(rawJsonResult))
            throw new ValidationException("Raw JSON result cannot be empty.");

        Duration = ScanDuration.Create(duration);
        RawJsonResult = rawJsonResult;
        Summary = summary ?? string.Empty;
        HasCompleted = true;
    }

    /// <summary>
    /// Marks the scan as incomplete (e.g., due to timeout or error).
    /// </summary>
    public void MarkAsIncomplete(string summary = "Scan did not complete successfully.")
    {
        HasCompleted = false;
        Summary = summary;
    }

    /// <summary>
    /// Updates the raw JSON result (e.g., for partial scan updates).
    /// </summary>
    public void UpdateRawJsonResult(string rawJsonResult)
    {
        if (string.IsNullOrWhiteSpace(rawJsonResult))
            throw new ValidationException("Raw JSON result cannot be empty.");

        RawJsonResult = rawJsonResult;
    }

    /// <summary>
    /// Updates the AI-generated summary.
    /// </summary>
    public void UpdateSummary(string summary)
    {
        Summary = summary ?? string.Empty;
    }
}
