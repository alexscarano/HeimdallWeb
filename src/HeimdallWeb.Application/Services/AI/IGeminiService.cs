namespace HeimdallWeb.Application.Services.AI;

/// <summary>
/// Service interface for Gemini AI vulnerability analysis.
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Analyzes scan results using Gemini AI and returns structured vulnerability findings.
    /// </summary>
    /// <param name="scanJson">JSON string containing scan results from all scanners</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON string with AI analysis including findings and technologies</returns>
    Task<string> AnalyzeScanResultsAsync(string scanJson, CancellationToken cancellationToken = default);
}
