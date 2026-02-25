namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for AI-generated summary.
/// Contains categorized risk assessment from Gemini AI.
/// </summary>
public record IASummaryResponse(
    int IASummaryId,
    string? SummaryText,
    string? MainCategory,
    string? OverallRisk,
    int TotalFindings,
    int FindingsCritical,
    int FindingsHigh,
    int FindingsMedium,
    int FindingsLow,
    int? HistoryId,
    DateTime CreatedDate
);
