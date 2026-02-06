namespace HeimdallWeb.Application.Services;

/// <summary>
/// Service interface for coordinating all security scanners.
/// </summary>
public interface IScannerService
{
    /// <summary>
    /// Runs all security scanners against the target and returns combined JSON results.
    /// </summary>
    /// <param name="target">The URL or IP address to scan</param>
    /// <param name="cancellationToken">Cancellation token for timeout/cancellation</param>
    /// <returns>Combined JSON string with all scanner results</returns>
    Task<string> RunAllScannersAsync(string target, CancellationToken cancellationToken);
}
