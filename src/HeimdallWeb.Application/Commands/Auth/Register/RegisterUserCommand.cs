namespace HeimdallWeb.Application.Commands.Auth.Register;

/// <summary>
/// Command to register a new user account.
/// </summary>
public record RegisterUserCommand(
    string Email,
    string Username,
    string Password,
    string RemoteIp
);
