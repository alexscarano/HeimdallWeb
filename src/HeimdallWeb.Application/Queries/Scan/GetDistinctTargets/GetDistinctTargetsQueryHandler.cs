using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Scan.GetDistinctTargets;

/// <summary>
/// Handles GetDistinctTargetsQuery.
/// Resolves user public UUID to internal ID, then delegates to repository.
/// </summary>
public class GetDistinctTargetsQueryHandler : IQueryHandler<GetDistinctTargetsQuery, IEnumerable<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDistinctTargetsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<string>> Handle(GetDistinctTargetsQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", query.UserId);

        return await _unitOfWork.ScanHistories.GetDistinctTargetsAsync(user.UserId, cancellationToken);
    }
}
