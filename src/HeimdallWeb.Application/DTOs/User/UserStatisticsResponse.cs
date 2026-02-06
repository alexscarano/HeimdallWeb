namespace HeimdallWeb.Application.DTOs.User;

/// <summary>
/// Response DTO for user scan statistics.
/// Used in GetUserStatisticsQuery.
/// </summary>
public record UserStatisticsResponse(
    // Scan Summary
    int TotalScans,
    int CompletedScans,
    int IncompleteScans,
    string? AverageDuration,
    string? LastScanDate,

    // Findings Summary
    int TotalFindings,
    int CriticalFindings,
    int HighFindings,
    int MediumFindings,
    int LowFindings,
    int InformationalFindings,

    // Trends (simplified for Phase 3)
    List<RiskTrendItem> RiskTrend,
    List<CategoryBreakdownItem> CategoryBreakdown
);

/// <summary>
/// Risk trend item showing findings count per day.
/// </summary>
/// <param name="Date">Date in "yyyy-MM-dd" format</param>
/// <param name="FindingsCount">Number of findings on that date</param>
public record RiskTrendItem(
    string Date,
    int FindingsCount
);

/// <summary>
/// Category breakdown item showing findings by category.
/// </summary>
/// <param name="Category">Finding category/type</param>
/// <param name="Count">Number of findings in this category</param>
public record CategoryBreakdownItem(
    string Category,
    int Count
);
