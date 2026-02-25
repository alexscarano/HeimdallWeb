namespace HeimdallWeb.Application.Notifications.Queries;

/// <summary>
/// Query for retrieving a paginated list of notifications for a specific user.
/// </summary>
/// <param name="UserId">Internal integer ID of the requesting user.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Number of notifications per page.</param>
public record GetNotificationsQuery(int UserId, int Page = 1, int PageSize = 10);
