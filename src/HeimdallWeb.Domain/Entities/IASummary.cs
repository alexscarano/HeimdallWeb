using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// IASummary entity representing AI-generated analysis of scan results.
/// </summary>
public class IASummary
{
    public int IASummaryId { get; private set; }
    public string? SummaryText { get; private set; }
    public string? MainCategory { get; private set; }
    public string? OverallRisk { get; private set; }
    public int TotalFindings { get; private set; }
    public int FindingsCritical { get; private set; }
    public int FindingsHigh { get; private set; }
    public int FindingsMedium { get; private set; }
    public int FindingsLow { get; private set; }
    public string? IANotes { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public int? HistoryId { get; private set; }

    // Navigation property (parent)
    public ScanHistory? History { get; private set; }

    // Parameterless constructor for EF Core
    private IASummary() { }

    /// <summary>
    /// Creates a new IASummary instance.
    /// </summary>
    public IASummary(
        string? summaryText,
        string? mainCategory,
        string? overallRisk,
        int totalFindings,
        int findingsCritical,
        int findingsHigh,
        int findingsMedium,
        int findingsLow,
        string? iaNotes = null,
        int? historyId = null)
    {
        if (totalFindings < 0)
            throw new ValidationException("Total findings cannot be negative.");

        if (findingsCritical < 0 || findingsHigh < 0 || findingsMedium < 0 || findingsLow < 0)
            throw new ValidationException("Finding counts cannot be negative.");

        SummaryText = summaryText;
        MainCategory = mainCategory;
        OverallRisk = overallRisk;
        TotalFindings = totalFindings;
        FindingsCritical = findingsCritical;
        FindingsHigh = findingsHigh;
        FindingsMedium = findingsMedium;
        FindingsLow = findingsLow;
        IANotes = iaNotes;
        CreatedDate = DateTime.UtcNow;
        HistoryId = historyId;
    }

    /// <summary>
    /// Updates the AI-generated summary text.
    /// </summary>
    public void UpdateSummary(string? summaryText, string? iaNotes = null)
    {
        SummaryText = summaryText;
        IANotes = iaNotes;
    }
}
