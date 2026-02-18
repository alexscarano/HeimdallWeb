using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

public class CspAnalyzerScanner : IScanner
{
    public ScannerMetadata Metadata => new(
        Key: "CSP",
        DisplayName: "Content Security Policy Analyzer",
        Category: "Headers",
        DefaultTimeout: TimeSpan.FromSeconds(8));

    public async Task<JObject> ScanAsync(string target, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(7));

            using var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            using var client = new HttpClient(handler);

            var response = await client.GetAsync(target, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            var issues = new List<string>();
            var alerts = new JArray();

            if (!response.Headers.TryGetValues("Content-Security-Policy", out var cspValues))
            {
                alerts.Add("Content-Security-Policy header is absent — browsers will apply no content restrictions");
                return new JObject
                {
                    ["csp_analysis"] = new JObject
                    {
                        ["csp_present"] = false,
                        ["raw_value"] = JValue.CreateNull(),
                        ["issues"] = new JArray(),
                        ["alerts"] = alerts
                    }
                };
            }

            var rawValue = string.Join("; ", cspValues);
            AnalyzeCsp(rawValue, issues);

            foreach (var issue in issues)
                alerts.Add($"CSP issue: {issue}");

            return new JObject
            {
                ["csp_analysis"] = new JObject
                {
                    ["csp_present"] = true,
                    ["raw_value"] = rawValue,
                    ["issues"] = new JArray(issues),
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
                ["csp_analysis"] = new JObject
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    private static void AnalyzeCsp(string rawValue, List<string> issues)
    {
        // Parse directives: "directive-name value1 value2; ..."
        var directives = rawValue
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => d.Trim())
            .Where(d => !string.IsNullOrEmpty(d))
            .ToDictionary(
                d => d.Split(' ', 2)[0].ToLowerInvariant(),
                d => d.Contains(' ') ? d.Split(' ', 2)[1] : string.Empty,
                StringComparer.OrdinalIgnoreCase);

        if (!directives.ContainsKey("default-src"))
            issues.Add("missing default-src directive");

        if (directives.TryGetValue("script-src", out var scriptSrc))
        {
            if (scriptSrc.Contains("'unsafe-inline'", StringComparison.OrdinalIgnoreCase))
                issues.Add("unsafe-inline in script-src");
            if (scriptSrc.Contains("'unsafe-eval'", StringComparison.OrdinalIgnoreCase))
                issues.Add("unsafe-eval in script-src");
        }

        if (directives.TryGetValue("style-src", out var styleSrc))
        {
            if (styleSrc.Contains("'unsafe-inline'", StringComparison.OrdinalIgnoreCase))
                issues.Add("unsafe-inline in style-src");
        }

        // Check for wildcard '*' in any *-src directive
        foreach (var (key, value) in directives)
        {
            if (key.EndsWith("-src", StringComparison.OrdinalIgnoreCase))
            {
                var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Any(t => t == "*"))
                    issues.Add($"wildcard '*' in {key} allows any origin");
            }
        }
    }
}
