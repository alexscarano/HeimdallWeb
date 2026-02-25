using HeimdallWeb.Application.Services.Scanners;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Application.Services;

/// <summary>
/// Application service that delegates scanner orchestration to ScannerManager.
/// ScannerManager now runs all scanners in parallel with individual timeouts.
/// </summary>
public class ScannerService : IScannerService
{
    private readonly ILogger<ScannerManager> _scannerLogger;

    public ScannerService(ILogger<ScannerManager> scannerLogger)
    {
        _scannerLogger = scannerLogger;
    }

    public async Task<string> RunAllScannersAsync(string target, CancellationToken cancellationToken, IEnumerable<string>? enabledScanners = null)
    {
        var scannerManager = new ScannerManager(_scannerLogger);
        var result = await scannerManager.RunAllAsync(target, cancellationToken, enabledScanners);
        return result.ToString();
    }
}
