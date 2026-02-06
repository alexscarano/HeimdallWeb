using HeimdallWeb.Application.Services.Scanners;

namespace HeimdallWeb.Application.Services;

/// <summary>
/// Service for coordinating all security scanners.
/// Wraps the ScannerManager from the legacy codebase.
/// </summary>
public class ScannerService : IScannerService
{
    public async Task<string> RunAllScannersAsync(string target, CancellationToken cancellationToken)
    {
        var scannerManager = new ScannerManager();
        var result = await scannerManager.RunAllAsync(target, cancellationToken);
        return result.ToString();
    }
}
