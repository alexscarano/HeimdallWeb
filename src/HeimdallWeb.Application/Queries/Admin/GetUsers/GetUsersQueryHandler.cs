using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Admin;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Admin.GetUsers;

/// <summary>
/// Handler for GetUsersQuery.
/// Returns paginated user list with filters (admin only).
/// Source: HeimdallWebOld/Controllers/AdminController.cs lines 67-90 (GerenciarUsuarios method)
/// </summary>
public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, PaginatedUsersResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<PaginatedUsersResponse> Handle(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        // Verify requesting user is admin
        var requestingUser = await _unitOfWork.Users.GetByPublicIdAsync(query.RequestingUserId, cancellationToken);
        if (requestingUser == null)
            throw new NotFoundException("User", query.RequestingUserId);

        if (requestingUser.UserType != UserType.Admin)
            throw new ForbiddenException("Admin access required");

        // Validate and cap page size
        var pageSize = Math.Min(query.PageSize, 50);
        if (pageSize <= 0) pageSize = 10;

        var page = query.Page <= 0 ? 1 : query.Page;

        // Get paginated users with filters
        var (users, totalCount) = await _unitOfWork.Users.GetPaginatedAsync(
            page,
            pageSize,
            query.SearchTerm,
            query.IsActive,
            query.IsAdmin,
            query.CreatedFrom,
            query.CreatedTo,
            cancellationToken);

        // For each user, get scan count and findings count
        var usersList = new List<UserListItemResponse>();

        foreach (var user in users)
        {
            var scanCount = await _unitOfWork.ScanHistories.CountByUserIdAsync(user.UserId, cancellationToken);
            var findingsCount = await _unitOfWork.Findings.CountByUserIdAsync(user.UserId, cancellationToken);

            usersList.Add(new UserListItemResponse(
                UserId: user.PublicId,
                Username: user.Username,
                Email: user.Email.Value,
                UserType: (int)user.UserType,
                IsActive: user.IsActive,
                ProfileImage: user.ProfileImage,
                CreatedAt: user.CreatedAt,
                ScanCount: scanCount,
                FindingsCount: findingsCount
            ));
        }

        // Calculate pagination metadata
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasNextPage = page < totalPages;
        var hasPreviousPage = page > 1;

        return new PaginatedUsersResponse(
            Users: usersList,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages,
            HasNextPage: hasNextPage,
            HasPreviousPage: hasPreviousPage
        );
    }
}
