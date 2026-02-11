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
        // Verify scan history exists by PublicId
        var scanHistory = await _unitOfWork.ScanHistories.GetByPublicIdAsync(query.HistoryId, cancellationToken);

        if (scanHistory == null)
            throw new NotFoundException("Scan history", query.HistoryId);

        // Verify ownership (users can only view their own technologies, admins can view any)
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.RequestingUserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", query.RequestingUserId);

        if (user.UserType != UserType.Admin && scanHistory.UserId != user.UserId)
            throw new NotFoundException("Scan history", query.HistoryId); // Security: 404 instead of 403

        // Get technologies using internal HistoryId
        var technologies = await _unitOfWork.Technologies.GetByHistoryIdAsync(scanHistory.HistoryId, cancellationToken);

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
