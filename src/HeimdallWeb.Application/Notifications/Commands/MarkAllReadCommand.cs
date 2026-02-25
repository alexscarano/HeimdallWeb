namespace HeimdallWeb.Application.Notifications.Commands;

/// <summary>
/// Command for marking all notifications belonging to a user as read in a single operation.
/// </summary>
/// <param name="UserId">Internal integer ID of the requesting user.</param>
public record MarkAllReadCommand(int UserId);
