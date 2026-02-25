namespace HeimdallWeb.Application.Queries.Scan.GetDistinctTargets;

/// <summary>
/// Query to get all distinct scan target URLs for the authenticated user.
/// Used to populate autocomplete suggestions in the monitor page.
/// </summary>
/// <param name="UserId">The user's public UUID from the JWT claim.</param>
public record GetDistinctTargetsQuery(Guid UserId);
