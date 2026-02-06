using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Scan.GetTechnologiesByHistoryId;

/// <summary>
/// Handler for GetTechnologiesByHistoryIdQuery.
/// Retrieves all detected technologies for a scan, ordered by Category then by Name.
/// Validates ownership before returning data.
///
/// Source: HeimdallWebOld/Controllers/HistoryController.cs lines 109-124 (GetTechnologies method)
/// </summary>
public class GetTechnologiesByHistoryIdQueryHandler : IQueryHandler<GetTechnologiesByHistoryIdQuery, IEnumerable<TechnologyResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTechnologiesByHistoryIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<TechnologyResponse>> Handle(GetTechnologiesByHistoryIdQuery query, CancellationToken cancellationToken = default)
    {
        // Verify scan history exists
        var scanHistory = await _unitOfWork.ScanHistories.GetByIdAsync(query.HistoryId, cancellationToken);

        if (scanHistory == null)
            throw new NotFoundException($"Scan history with ID {query.HistoryId} not found");

        // Verify ownership (users can only view their own technologies, admins can view any)
        var user = await _unitOfWork.Users.GetByIdAsync(query.RequestingUserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", query.RequestingUserId);

        if (user.UserType != UserType.Admin && scanHistory.UserId != query.RequestingUserId)
            throw new ForbiddenException("You can only view technologies from your own scan history");

        // Get technologies
        var technologies = await _unitOfWork.Technologies.GetByHistoryIdAsync(query.HistoryId, cancellationToken);

        // Order by Category, then by Name
        var orderedTechnologies = technologies
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToList();

        // Map to response DTOs
        return orderedTechnologies.Select(t => new TechnologyResponse(
            TechnologyId: t.TechnologyId,
            Name: t.Name,
            Version: t.Version,
            Category: t.Category,
            Description: t.Description,
            HistoryId: t.HistoryId,
            CreatedAt: t.CreatedAt
        ));
    }
}
