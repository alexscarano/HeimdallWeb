namespace HeimdallWeb.Application.Queries.Scan.GetFindingsByHistoryId;

/// <summary>
/// Query to retrieve all security findings for a specific scan history.
/// Validates ownership before returning data.
/// </summary>
/// <param name="HistoryId">The scan history ID</param>
/// <param name="RequestingUserId">The user requesting the data (for ownership verification)</param>
public record GetFindingsByHistoryIdQuery(
    Guid HistoryId,
    Guid RequestingUserId
);
