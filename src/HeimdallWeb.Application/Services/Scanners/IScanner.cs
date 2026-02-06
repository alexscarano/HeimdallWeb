using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

/// <summary>
/// Interface for security scanners.
/// Each scanner performs a specific type of security analysis.
/// </summary>
public interface IScanner
{
    /// <summary>
    /// Performs a security scan on the target.
    /// </summary>
    /// <param name="target">The URL or IP address to scan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON object with scan results</returns>
    Task<JObject> ScanAsync(string target, CancellationToken cancellationToken = default);
}
