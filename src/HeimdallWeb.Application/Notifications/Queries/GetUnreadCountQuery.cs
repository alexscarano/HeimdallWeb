namespace HeimdallWeb.Application.Notifications.Queries;

/// <summary>
/// Query for retrieving the total number of unread notifications for a specific user.
/// </summary>
/// <param name="UserId">Internal integer ID of the requesting user.</param>
public record GetUnreadCountQuery(int UserId);
