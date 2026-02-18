using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Scan.GetScanProfiles;

/// <summary>
/// Handler for GetScanProfilesQuery.
/// Returns all available scan profiles (system + custom), ordered by name.
/// This endpoint is intentionally open (no auth required) so the UI can
/// display profile options on the scan form without requiring a token.
/// </summary>
public class GetScanProfilesQueryHandler : IQueryHandler<GetScanProfilesQuery, IEnumerable<ScanProfileResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetScanProfilesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<ScanProfileResponse>> Handle(
        GetScanProfilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var profiles = await _unitOfWork.ScanProfiles.GetAllAsync(cancellationToken);

        return profiles.Select(p => new ScanProfileResponse(
            Id: p.Id,
            Name: p.Name,
            Description: p.Description,
            ConfigJson: p.ConfigJson,
            IsSystem: p.IsSystem
        ));
    }
}
