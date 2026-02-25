using HeimdallWeb.Domain.Enums;

namespace HeimdallWeb.Application.Commands.Admin.ToggleUserStatus;

/// <summary>
/// Command to toggle (activate/deactivate) a user's account status.
/// Admin-only operation with business rules:
/// - Requesting user must be Admin
/// - Cannot toggle admin users
/// </summary>
public record ToggleUserStatusCommand(
    Guid UserId,
    bool IsActive,
    UserType RequestingUserType
);
