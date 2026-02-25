using HeimdallWeb.Domain.Enums;

namespace HeimdallWeb.Application.Commands.Monitor.CreateMonitor;

/// <summary>
/// Command for registering a URL to be periodically monitored by the system.
/// </summary>
/// <param name="UserPublicId">User's public UUID from JWT claims.</param>
/// <param name="Url">URL to monitor.</param>
/// <param name="Frequency">How often the target should be scanned.</param>
public record CreateMonitorCommand(Guid UserPublicId, string Url, MonitorFrequency Frequency);
