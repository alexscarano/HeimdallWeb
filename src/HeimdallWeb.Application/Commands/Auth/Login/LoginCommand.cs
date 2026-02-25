namespace HeimdallWeb.Application.Commands.Auth.Login;

/// <summary>
/// Command to authenticate a user with email/username and password.
/// </summary>
public record LoginCommand(
    string EmailOrUsername,
    string Password,
    string RemoteIp
);
