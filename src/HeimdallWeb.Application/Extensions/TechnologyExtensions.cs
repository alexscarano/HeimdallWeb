using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Application.Extensions;

/// <summary>
/// Extension methods for mapping Technology entity to DTOs.
/// </summary>
public static class TechnologyExtensions
{
    /// <summary>
    /// Maps Technology entity to TechnologyResponse DTO.
    /// </summary>
    /// <param name="technology">Technology entity</param>
    /// <returns>TechnologyResponse DTO</returns>
    public static TechnologyResponse ToDto(this Technology technology)
    {
        return new TechnologyResponse(
            TechnologyId: technology.TechnologyId,
            Name: technology.Name,
            Version: technology.Version,
            Category: technology.Category,
            Description: technology.Description,
            HistoryId: technology.HistoryId,
            CreatedAt: technology.CreatedAt
        );
    }
}
