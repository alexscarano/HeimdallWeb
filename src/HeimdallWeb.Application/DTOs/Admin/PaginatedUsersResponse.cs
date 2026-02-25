namespace HeimdallWeb.Application.DTOs.Admin;

/// <summary>
/// Response DTO for paginated user list.
/// Used in GetUsersQuery (admin only).
/// </summary>
public record PaginatedUsersResponse(
    List<UserListItemResponse> Users,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);

/// <summary>
/// Individual user list item with scan/findings counts.
/// </summary>
public record UserListItemResponse(
    Guid UserId,
    string Username,
    string Email,
    int UserType,
    bool IsActive,
    string? ProfileImage,
    DateTime CreatedAt,
    int ScanCount,
    int FindingsCount
);
