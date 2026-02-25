namespace HeimdallWeb.Application.DTOs.Support;

/// <summary>
/// Request DTO for the support contact form endpoint.
/// </summary>
public record SendContactRequest(
    string Name,
    string Email,
    string Subject,
    string Message
);
