using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.User.GetUserProfile;

/// <summary>
/// Handler for GetUserProfileQuery.
/// Retrieves user profile details for viewing/editing.
/// Source: HeimdallWebOld/Controllers/UserController.cs lines 34-56 (Profile GET method)
/// </summary>
public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, UserProfileResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserProfileQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<UserProfileResponse> Handle(GetUserProfileQuery query, CancellationToken cancellationToken = default)
    {
        // Get user by PublicId
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", query.UserId);

        // Map to response DTO
        return new UserProfileResponse(
            UserId: user.PublicId,
            Username: user.Username,
            Email: user.Email.Value,
            UserType: (int)user.UserType,
            IsActive: user.IsActive,
            ProfileImage: user.ProfileImage,
            CreatedAt: user.CreatedAt
        );
    }
}
