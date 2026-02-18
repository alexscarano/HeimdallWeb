using HeimdallWeb.Application.DTOs.Scan;

namespace HeimdallWeb.Application.Commands.Scan.ExecuteScan;

/// <summary>
/// Command to execute a security scan on a target URL or IP address.
/// Extracted from ScanService.RunScanAndPersist (HeimdallWebOld).
/// ProfileId is optional — when present the scan is tagged with the chosen profile.
/// When absent all scanners run with default settings (backward compatible).
/// </summary>
public record ExecuteScanCommand(
    string Target,
    Guid UserId,
    string RemoteIp,
    int? ProfileId = null,
    IEnumerable<string>? EnabledScanners = null
);
