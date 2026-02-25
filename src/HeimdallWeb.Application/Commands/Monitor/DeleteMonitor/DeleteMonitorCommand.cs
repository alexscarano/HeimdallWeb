namespace HeimdallWeb.Application.Commands.Monitor.DeleteMonitor;

/// <summary>
/// Command for removing a monitored target.
/// The caller must own the target; ownership is verified via the user's public UUID.
/// </summary>
/// <param name="MonitoredTargetId">Primary key of the target to delete.</param>
/// <param name="UserPublicId">User's public UUID from JWT claims, used to verify ownership.</param>
public record DeleteMonitorCommand(int MonitoredTargetId, Guid UserPublicId);
