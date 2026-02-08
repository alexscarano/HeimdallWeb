namespace HeimdallWeb.Application.Commands.User.UpdatePassword;

/// <summary>
/// Response for password update operation.
/// </summary>
public record UpdatePasswordResponse(
    bool Success,
    string Message
);
