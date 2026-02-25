namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Request DTO for executing a security scan.
/// ProfileId is optional — when provided the scan is associated with the named profile.
/// When omitted, all scanners run with default settings (backward compatible).
/// </summary>
public record ExecuteScanRequest(
    string Target,
    int? ProfileId = null,
    IEnumerable<string>? EnabledScanners = null
);
