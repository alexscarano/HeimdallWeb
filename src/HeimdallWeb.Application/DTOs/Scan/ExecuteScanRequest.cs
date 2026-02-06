namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Request DTO for executing a security scan.
/// </summary>
public record ExecuteScanRequest(
    string Target
);
