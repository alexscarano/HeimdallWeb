namespace HeimdallWeb.Application.Queries.Monitor.GetUserMonitors;

/// <summary>
/// Query for retrieving all monitored targets registered by a specific user.
/// </summary>
/// <param name="UserPublicId">User's public UUID from JWT claims.</param>
public record GetUserMonitorsQuery(Guid UserPublicId);
