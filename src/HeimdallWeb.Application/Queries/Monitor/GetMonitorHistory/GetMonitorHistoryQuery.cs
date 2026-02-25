namespace HeimdallWeb.Application.Queries.Monitor.GetMonitorHistory;

/// <summary>
/// Query for retrieving the risk snapshot history of a specific monitored target.
/// </summary>
/// <param name="MonitoredTargetId">Primary key of the monitored target.</param>
/// <param name="UserPublicId">User's public UUID from JWT claims, used to verify ownership.</param>
public record GetMonitorHistoryQuery(int MonitoredTargetId, Guid UserPublicId);
