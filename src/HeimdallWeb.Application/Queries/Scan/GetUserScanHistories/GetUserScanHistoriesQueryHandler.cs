using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Scan.GetUserScanHistories;

/// <summary>
/// Handler for GetUserScanHistoriesQuery.
/// Retrieves paginated scan histories for a user, ordered by created date DESC.
/// Includes finding and technology counts for each scan.
///
/// Source: HeimdallWebOld/Controllers/HistoryController.cs lines 32-41 (Index method)
/// </summary>
public class GetUserScanHistoriesQueryHandler : IQueryHandler<GetUserScanHistoriesQuery, PaginatedScanHistoriesResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private const int MaxPageSize = 50;

    public GetUserScanHistoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<PaginatedScanHistoriesResponse> Handle(GetUserScanHistoriesQuery query, CancellationToken cancellationToken = default)
    {
        // Validate user exists and resolve to internal ID
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", query.UserId);

        var userInternalId = user.UserId; // Use internal ID for FK query

        // Enforce page size limits
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        // Get paginated histories with counts already included (N+1 problem solved)
        var (summaryResponses, totalCount) = await _unitOfWork.ScanHistories.GetByUserIdPaginatedAsync(
            userInternalId,
            page,
            pageSize,
            cancellationToken);

        // Calculate pagination metadata
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasNextPage = page < totalPages;
        var hasPreviousPage = page > 1;

        return new PaginatedScanHistoriesResponse(
            Items: summaryResponses,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages,
            HasNextPage: hasNextPage,
            HasPreviousPage: hasPreviousPage
        );
    }
}
