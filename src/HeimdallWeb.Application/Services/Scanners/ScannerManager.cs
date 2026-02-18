using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

/// <summary>
/// Manages and orchestrates all security scanners in parallel.
/// Each scanner runs concurrently with its own individual timeout so a hung scanner
/// cannot block the entire pipeline. Results are merged into a single JObject.
/// </summary>
public class ScannerManager
{
    private readonly IList<IScanner> _scanners;
    private readonly ILogger<ScannerManager>? _logger;

    public ScannerManager(ILogger<ScannerManager>? logger = null)
    {
        _logger = logger;
        _scanners = new List<IScanner>
        {
            new HeaderScanner(),
            new SslScanner(),
            new PortScanner(),
            new SensitivePathsScanner(),
            new HttpRedirectScanner(),
            new RobotsScanner(),
        };
    }

    /// <summary>
    /// Runs all scanners in parallel, each with its individual timeout.
    /// Failed or timed-out scanners are logged and skipped — they do not abort the pipeline.
    /// </summary>
    /// <param name="target">The target URL.</param>
    /// <param name="globalCancellationToken">
    /// Global cancellation token (e.g. the 75-second request timeout).
    /// Each scanner also gets a linked individual timeout from its Metadata.DefaultTimeout.
    /// </param>
    public async Task<JObject> RunAllAsync(string target, CancellationToken globalCancellationToken = default)
    {
        var tasks = _scanners.Select(scanner => RunSingleScannerAsync(scanner, target, globalCancellationToken));

        var results = await Task.WhenAll(tasks);

        var merged = new JObject();
        foreach (var result in results.Where(r => r is not null))
        {
            merged.Merge(result!, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });
        }

        return merged;
    }

    /// <summary>
    /// Executes a single scanner wrapped in its individual timeout.
    /// Returns null if the scanner fails or times out (non-fatal).
    /// </summary>
    private async Task<JObject?> RunSingleScannerAsync(
        IScanner scanner,
        string target,
        CancellationToken globalCt)
    {
        using var individualCts = CancellationTokenSource.CreateLinkedTokenSource(globalCt);
        individualCts.CancelAfter(scanner.Metadata.DefaultTimeout);

        try
        {
            return await scanner.ScanAsync(target, individualCts.Token);
        }
        catch (OperationCanceledException) when (!globalCt.IsCancellationRequested)
        {
            // Individual scanner timed out — log and continue
            _logger?.LogWarning(
                "[ScannerManager] Scanner '{Key}' timed out after {Timeout}s. Skipping.",
                scanner.Metadata.Key,
                scanner.Metadata.DefaultTimeout.TotalSeconds);
            return null;
        }
        catch (Exception ex)
        {
            // Scanner threw an unexpected error — log and continue
            _logger?.LogError(
                ex,
                "[ScannerManager] Scanner '{Key}' failed with error: {Message}",
                scanner.Metadata.Key,
                ex.Message);
            return null;
        }
    }
}
