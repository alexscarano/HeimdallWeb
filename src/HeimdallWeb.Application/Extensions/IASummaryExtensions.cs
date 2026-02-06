using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Application.Extensions;

/// <summary>
/// Extension methods for mapping IASummary entity to DTOs.
/// </summary>
public static class IASummaryExtensions
{
    /// <summary>
    /// Maps IASummary entity to IASummaryResponse DTO.
    /// </summary>
    /// <param name="iaSummary">IASummary entity</param>
    /// <returns>IASummaryResponse DTO</returns>
    public static IASummaryResponse ToDto(this IASummary iaSummary)
    {
        return new IASummaryResponse(
            IASummaryId: iaSummary.IASummaryId,
            SummaryText: iaSummary.SummaryText,
            MainCategory: iaSummary.MainCategory,
            OverallRisk: iaSummary.OverallRisk,
            TotalFindings: iaSummary.TotalFindings,
            FindingsCritical: iaSummary.FindingsCritical,
            FindingsHigh: iaSummary.FindingsHigh,
            FindingsMedium: iaSummary.FindingsMedium,
            FindingsLow: iaSummary.FindingsLow,
            HistoryId: iaSummary.HistoryId,
            CreatedDate: iaSummary.CreatedDate
        );
    }
}
