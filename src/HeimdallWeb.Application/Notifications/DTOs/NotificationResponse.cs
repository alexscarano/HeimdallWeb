namespace HeimdallWeb.Application.Notifications.DTOs;

/// <summary>
/// Response DTO for a single notification.
/// </summary>
/// <param name="Id">Primary key of the notification.</param>
/// <param name="Title">Short title describing the notification event.</param>
/// <param name="Body">Full notification message body.</param>
/// <param name="Type">Notification type identifier (e.g. ScanComplete, Alert).</param>
/// <param name="IsRead">Whether the user has already read this notification.</param>
/// <param name="CreatedAt">UTC timestamp when the notification was created.</param>
/// <param name="ReadAt">UTC timestamp when the notification was marked as read; null if unread.</param>
public record NotificationResponse(
    int Id,
    string Title,
    string Body,
    string Type,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt
);
