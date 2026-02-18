using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

public class ResponseBehaviorScanner : IScanner
{
    public ScannerMetadata Metadata => new(
        Key: "ResponseBehavior",
        DisplayName: "Response Behavior",
        Category: "General",
        DefaultTimeout: TimeSpan.FromSeconds(15));

    private const string NotFoundPath = "/nonexistent-path-404-test-xyz";

    public async Task<JObject> ScanAsync(string target, CancellationToken cancellationToken = default)
    {
        try
        {
            using var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            using var client = new HttpClient(handler);

            cancellationToken.ThrowIfCancellationRequested();

            // Measure TTFB for the home page
            long ttfbMs = await MeasureTtfbAsync(client, target, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // Check home page status code
            int homeStatusCode = await GetStatusCodeAsync(client, target, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // Check 404 handling
            int notFoundStatus = await GetStatusCodeAsync(client, $"{target.TrimEnd('/')}{NotFoundPath}", cancellationToken);
            bool returnsProper404 = notFoundStatus == 404;

            var alerts = new JArray();

            if (ttfbMs > 2000)
                alerts.Add($"High Time To First Byte: {ttfbMs}ms (threshold: 2000ms) — may indicate server-side performance issues");

            if (!returnsProper404)
                alerts.Add($"Soft 404 detected: non-existent path returned HTTP {notFoundStatus} instead of 404 — search engines and crawlers may be misled");

            return new JObject
            {
                ["response_behavior"] = new JObject
                {
                    ["ttfb_ms"] = ttfbMs,
                    ["returns_proper_404"] = returnsProper404,
                    ["home_status_code"] = homeStatusCode,
                    ["alerts"] = alerts
                }
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new JObject
            {
                ["response_behavior"] = new JObject
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    private static async Task<long> MeasureTtfbAsync(HttpClient client, string target, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // ResponseHeadersRead stops as soon as headers are received — a good TTFB proxy
            var response = await client.GetAsync(target, HttpCompletionOption.ResponseHeadersRead, ct);
            sw.Stop();
            response.Dispose();
            return sw.ElapsedMilliseconds;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
    }

    private static async Task<int> GetStatusCodeAsync(HttpClient client, string url, CancellationToken ct)
    {
        try
        {
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            var code = (int)response.StatusCode;
            response.Dispose();
            return code;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return 0;
        }
    }
}
