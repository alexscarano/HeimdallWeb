namespace HeimdallWeb.Application.Queries.Scan.GetTechnologiesByHistoryId;

/// <summary>
/// Query to retrieve all detected technologies for a specific scan history.
/// Validates ownership before returning data.
/// </summary>
/// <param name="HistoryId">The scan history ID</param>
/// <param name="RequestingUserId">The user requesting the data (for ownership verification)</param>
public record GetTechnologiesByHistoryIdQuery(
    Guid HistoryId,
    Guid RequestingUserId
);
