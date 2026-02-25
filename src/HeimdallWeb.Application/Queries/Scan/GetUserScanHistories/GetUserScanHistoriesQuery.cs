namespace HeimdallWeb.Application.Queries.Scan.GetUserScanHistories;

/// <summary>
/// Query to retrieve paginated scan histories for a specific user.
/// Results are ordered by CreatedDate DESC (newest first).
/// </summary>
/// <param name="UserId">The user ID to retrieve scan histories for</param>
/// <param name="Page">Page number (1-based, default: 1)</param>
/// <param name="PageSize">Number of items per page (default: 10, max: 50)</param>
/// <param name="Search">Optional URL search filter (case-insensitive partial match)</param>
/// <param name="Status">Optional status filter: "completed", "failed", or null for all</param>
public record GetUserScanHistoriesQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Status = null
);
