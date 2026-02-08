using HeimdallWeb.Application.DTOs.Scan;

namespace HeimdallWeb.Application.Commands.Scan.ExecuteScan;

/// <summary>
/// Command to execute a security scan on a target URL or IP address.
/// Extracted from ScanService.RunScanAndPersist (HeimdallWebOld).
/// </summary>
public record ExecuteScanCommand(
    string Target,
    Guid UserId,
    string RemoteIp
);
