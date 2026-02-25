namespace HeimdallWeb.Application.Services;

/// <summary>
/// Service interface for coordinating all security scanners.
/// </summary>
public interface IScannerService
{
    /// <summary>
    /// Runs security scanners against the target and returns combined JSON results.
    /// When enabledScanners is null or empty, all scanners run (default behavior).
    /// </summary>
    /// <param name="target">The URL or IP address to scan</param>
    /// <param name="cancellationToken">Cancellation token for timeout/cancellation</param>
    /// <param name="enabledScanners">Optional list of scanner keys to run. Null = all scanners.</param>
    /// <returns>Combined JSON string with all scanner results</returns>
    Task<string> RunAllScannersAsync(string target, CancellationToken cancellationToken, IEnumerable<string>? enabledScanners = null);
}
