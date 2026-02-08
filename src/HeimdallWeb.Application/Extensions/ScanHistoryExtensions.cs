using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.DTOs;
using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Application.Extensions;

/// <summary>
/// Extension methods for mapping ScanHistory entity to DTOs.
/// </summary>
public static class ScanHistoryExtensions
{
    /// <summary>
    /// Maps ScanHistory entity to ScanHistoryDetailResponse DTO.
    /// Includes all related entities: findings, technologies, and AI summary.
    /// </summary>
    /// <param name="scanHistory">ScanHistory entity with included navigation properties</param>
    /// <returns>ScanHistoryDetailResponse DTO</returns>
    public static ScanHistoryDetailResponse ToDetailDto(this ScanHistory scanHistory)
    {
        return new ScanHistoryDetailResponse(
            HistoryId: scanHistory.PublicId,
            Target: scanHistory.Target.Value, // ScanTarget is a Value Object
            RawJsonResult: scanHistory.RawJsonResult,
            CreatedDate: scanHistory.CreatedDate,
            UserId: scanHistory.User?.PublicId ?? Guid.Empty,
            Duration: scanHistory.Duration?.Value.ToString(@"hh\:mm\:ss"), // ScanDuration is a Value Object (TimeSpan)
            HasCompleted: scanHistory.HasCompleted,
            Summary: scanHistory.Summary,
            Findings: scanHistory.Findings.Select(f => f.ToDto()).ToList(),
            Technologies: scanHistory.Technologies.Select(t => t.ToDto()).ToList(),
            IASummary: scanHistory.IASummaries.FirstOrDefault()?.ToDto()
        );
    }

    /// <summary>
    /// Maps ScanHistory entity to ExecuteScanResponse DTO.
    /// Lightweight response after executing a scan.
    /// </summary>
    /// <param name="scanHistory">ScanHistory entity</param>
    /// <returns>ExecuteScanResponse DTO</returns>
    public static ExecuteScanResponse ToExecuteScanDto(this ScanHistory scanHistory)
    {
        return new ExecuteScanResponse(
            HistoryId: scanHistory.PublicId,
            Target: scanHistory.Target.Value,
            Summary: scanHistory.Summary,
            Duration: scanHistory.Duration?.Value ?? TimeSpan.Zero, // TimeSpan directly, default to Zero if null
            HasCompleted: scanHistory.HasCompleted,
            CreatedDate: scanHistory.CreatedDate
        );
    }

    /// <summary>
    /// Maps ScanHistory entity to ScanHistorySummaryResponse DTO.
    /// Lightweight response for list views.
    /// </summary>
    /// <param name="scanHistory">ScanHistory entity with included navigation properties</param>
    /// <returns>ScanHistorySummaryResponse DTO</returns>
    public static ScanHistorySummaryResponse ToSummaryDto(this ScanHistory scanHistory)
    {
        return new ScanHistorySummaryResponse(
            HistoryId: scanHistory.PublicId,
            Target: scanHistory.Target.Value,
            CreatedDate: scanHistory.CreatedDate,
            Duration: scanHistory.Duration?.Value.ToString(@"hh\:mm\:ss"),
            HasCompleted: scanHistory.HasCompleted,
            Summary: scanHistory.Summary,
            FindingsCount: scanHistory.Findings.Count,
            TechnologiesCount: scanHistory.Technologies.Count
        );
    }
}
