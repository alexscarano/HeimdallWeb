using HeimdallWeb.Domain.DTOs;

namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for paginated list of scan histories.
/// Includes pagination metadata for efficient data loading.
/// </summary>
public record PaginatedScanHistoriesResponse(
    IEnumerable<ScanHistorySummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);
