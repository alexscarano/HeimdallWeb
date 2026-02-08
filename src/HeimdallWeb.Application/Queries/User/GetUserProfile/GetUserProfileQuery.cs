namespace HeimdallWeb.Application.Queries.User.GetUserProfile;

/// <summary>
/// Query to retrieve user profile details.
/// Used for profile viewing/editing.
/// Source: HeimdallWebOld/Controllers/UserController.cs lines 34-56 (Profile GET method)
/// </summary>
/// <param name="UserId">The user ID to retrieve profile for</param>
public record GetUserProfileQuery(
    Guid UserId
);
