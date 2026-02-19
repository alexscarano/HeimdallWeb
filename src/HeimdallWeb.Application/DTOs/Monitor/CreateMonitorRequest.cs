namespace HeimdallWeb.Application.DTOs.Monitor;

/// <summary>
/// Request DTO for registering a URL to be periodically monitored.
/// </summary>
/// <param name="Url">The URL to monitor. Must be a valid HTTP/HTTPS address.</param>
/// <param name="Frequency">Scan frequency: "Daily" or "Weekly".</param>
public record CreateMonitorRequest(string Url, string Frequency);
