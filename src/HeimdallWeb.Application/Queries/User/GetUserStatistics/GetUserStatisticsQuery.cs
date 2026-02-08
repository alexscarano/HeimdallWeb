namespace HeimdallWeb.Application.Queries.User.GetUserStatistics;

/// <summary>
/// Query to retrieve user scan statistics.
/// Returns comprehensive statistics including scan counts, findings breakdown, and trends.
/// Source: HeimdallWebOld/Controllers/UserController.cs lines 59-70 (Statistics method)
/// </summary>
/// <param name="UserId">The user ID to retrieve statistics for</param>
public record GetUserStatisticsQuery(
    Guid UserId
);
