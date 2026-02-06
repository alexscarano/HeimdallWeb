using HeimdallWeb.Domain.Enums;

namespace HeimdallWeb.Application.Commands.Admin.DeleteUserByAdmin;

/// <summary>
/// Command to delete a user account by an administrator.
/// Admin-only operation with strict business rules:
/// - Requesting user must be Admin
/// - Cannot delete admin users
/// - Cannot delete yourself (RequestingUserId != UserId)
/// </summary>
public record DeleteUserByAdminCommand(
    int UserId,
    UserType RequestingUserType,
    int RequestingUserId
);
