namespace HeimdallWeb.Application.Queries.Admin.GetUsers;

/// <summary>
/// Query to retrieve paginated user list with filters.
/// Requires Admin role. Used for user management.
/// Source: HeimdallWebOld/Controllers/AdminController.cs lines 67-90 (GerenciarUsuarios method)
/// </summary>
/// <param name="RequestingUserId">The user requesting the list (for admin verification)</param>
/// <param name="Page">Page number (default 1)</param>
/// <param name="PageSize">Items per page (default 10, max 50)</param>
/// <param name="SearchTerm">Search in username or email (optional)</param>
/// <param name="IsActive">Filter by active status (optional)</param>
/// <param name="IsAdmin">Filter by user type (true = admin, false = regular, optional)</param>
/// <param name="CreatedFrom">Filter by created date from (optional)</param>
/// <param name="CreatedTo">Filter by created date to (optional)</param>
public record GetUsersQuery(
    int RequestingUserId,
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null,
    bool? IsAdmin = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null
);
