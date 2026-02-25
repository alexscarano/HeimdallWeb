namespace HeimdallWeb.Application.DTOs.Auth;

/// <summary>
/// Request DTO for the Google OAuth endpoint.
/// The IdToken is the id_token obtained from the Google Sign-In client SDK.
/// </summary>
public record GoogleAuthRequest(string IdToken);
