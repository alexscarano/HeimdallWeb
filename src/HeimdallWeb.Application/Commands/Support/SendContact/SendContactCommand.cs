namespace HeimdallWeb.Application.Commands.Support.SendContact;

/// <summary>
/// Command to submit a support contact form (anonymous endpoint).
/// </summary>
public record SendContactCommand(
    string Name,
    string Email,
    string Subject,
    string Message,
    string RemoteIp
);
