using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.Queries.Scan.GetScanProfiles;

namespace HeimdallWeb.WebApi.Endpoints;

/// <summary>
/// Minimal API endpoints for scan profiles.
/// </summary>
public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/profiles")
            .WithTags("Profiles");

        // GET /api/v1/profiles — list all available scan profiles (public, no auth required)
        group.MapGet("", GetScanProfiles)
            .AllowAnonymous()
            .WithSummary("Get all available scan profiles")
            .WithDescription(
                "Returns all scan profiles (system defaults and any custom profiles). " +
                "This endpoint is public so the scan form can populate the profile selector " +
                "without requiring the user to be authenticated.");

        return group;
    }

    private static async Task<IResult> GetScanProfiles(
        IQueryHandler<GetScanProfilesQuery, IEnumerable<ScanProfileResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetScanProfilesQuery();
        var result = await handler.Handle(query, cancellationToken);
        return Results.Ok(result);
    }
}
