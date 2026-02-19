namespace HeimdallWeb.Application.Notifications.Commands;

/// <summary>
/// Command for marking a single notification as read.
/// Ownership is verified: the notification must belong to the requesting user.
/// </summary>
/// <param name="NotificationId">Primary key of the notification to mark as read.</param>
/// <param name="UserId">Internal integer ID of the requesting user, used to verify ownership.</param>
public record MarkNotificationReadCommand(int NotificationId, int UserId);
