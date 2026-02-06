using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Application.Extensions;

/// <summary>
/// Extension methods for mapping Finding entity to DTOs.
/// </summary>
public static class FindingExtensions
{
    /// <summary>
    /// Maps Finding entity to FindingResponse DTO.
    /// </summary>
    /// <param name="finding">Finding entity</param>
    /// <returns>FindingResponse DTO</returns>
    public static FindingResponse ToDto(this Finding finding)
    {
        return new FindingResponse(
            FindingId: finding.FindingId,
            Type: finding.Type,
            Description: finding.Description,
            Severity: finding.Severity, // Enum is serialized correctly
            Evidence: finding.Evidence,
            Recommendation: finding.Recommendation,
            HistoryId: finding.HistoryId,
            CreatedAt: finding.CreatedAt
        );
    }
}
