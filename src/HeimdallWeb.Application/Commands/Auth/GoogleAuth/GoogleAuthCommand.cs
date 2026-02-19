namespace HeimdallWeb.Application.Commands.Auth.GoogleAuth;

/// <summary>
/// Command to authenticate or register a user via Google OAuth.
/// The IdToken is validated against Google's public tokeninfo endpoint.
/// </summary>
public record GoogleAuthCommand(
    string IdToken,
    string RemoteIp
);
