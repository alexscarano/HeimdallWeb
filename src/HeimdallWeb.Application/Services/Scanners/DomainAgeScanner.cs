using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using HeimdallWeb.Application.Helpers;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

public class DomainAgeScanner : IScanner
{
    public ScannerMetadata Metadata => new(
        Key: "DomainAge",
        DisplayName: "Domain Age",
        Category: "General",
        DefaultTimeout: TimeSpan.FromSeconds(10));

    private static readonly Regex CreationDateRegex = new(
        @"(?:creation\s+date|created)\s*:\s*(.+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ReferRegex = new(
        @"refer\s*:\s*(.+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<JObject> ScanAsync(string targetRaw, CancellationToken cancellationToken = default)
    {
        try
        {
            var hostname = NetworkUtils.RemoveHttpString(targetRaw);

            // Strip www. prefix to get the registrable domain
            var domain = hostname.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                ? hostname.Substring(4)
                : hostname;

            cancellationToken.ThrowIfCancellationRequested();

            // Step 1: query whois.iana.org to find the TLD's WHOIS server
            var ianaResponse = await QueryWhoisAsync("whois.iana.org", domain, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            string whoisServer = "whois.iana.org";
            var referMatch = ReferRegex.Match(ianaResponse);
            if (referMatch.Success)
            {
                var candidate = referMatch.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(candidate))
                    whoisServer = candidate;
            }

            // Step 2: query the TLD-specific WHOIS server for the full domain record
            string whoisResponse = ianaResponse;
            if (!string.Equals(whoisServer, "whois.iana.org", StringComparison.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();
                whoisResponse = await QueryWhoisAsync(whoisServer, domain, cancellationToken);
            }

            // Step 3: extract creation date
            var match = CreationDateRegex.Match(whoisResponse);
            if (!match.Success)
            {
                return new JObject
                {
                    ["domain_age"] = new JObject
                    {
                        ["error"] = "WHOIS lookup failed: creation date not found in WHOIS response"
                    }
                };
            }

            var rawDate = match.Groups[1].Value.Trim();

            // Attempt to parse various date formats returned by WHOIS servers
            DateTime? creationDate = TryParseWhoisDate(rawDate);
            if (creationDate is null)
            {
                return new JObject
                {
                    ["domain_age"] = new JObject
                    {
                        ["error"] = $"WHOIS lookup failed: could not parse creation date '{rawDate}'"
                    }
                };
            }

            var ageDays = (int)(DateTime.UtcNow - creationDate.Value).TotalDays;

            var alerts = new JArray();
            if (ageDays < 90)
                alerts.Add($"Domain is very new ({ageDays} days old) — exercise caution");
            else if (ageDays < 365)
                alerts.Add($"Domain is less than 1 year old ({ageDays} days)");

            return new JObject
            {
                ["domain_age"] = new JObject
                {
                    ["domain"] = domain,
                    ["creation_date"] = creationDate.Value.ToString("yyyy-MM-dd"),
                    ["age_days"] = ageDays,
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
                ["domain_age"] = new JObject
                {
                    ["error"] = $"WHOIS lookup failed: {ex.Message}"
                }
            };
        }
    }

    private static async Task<string> QueryWhoisAsync(string server, string domain, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(8));

        using var tcp = new TcpClient();
        await tcp.ConnectAsync(server, 43, cts.Token);

        var query = Encoding.ASCII.GetBytes($"{domain}\r\n");
        await tcp.GetStream().WriteAsync(query, cts.Token);

        using var reader = new StreamReader(tcp.GetStream(), Encoding.ASCII);
        var sb = new StringBuilder();

        char[] buffer = new char[4096];
        int read;
        while ((read = await reader.ReadAsync(buffer, cts.Token)) > 0)
        {
            sb.Append(buffer, 0, read);

            // Guard against excessively large responses
            if (sb.Length > 65536)
                break;
        }

        return sb.ToString();
    }

    private static DateTime? TryParseWhoisDate(string raw)
    {
        // WHOIS dates come in many formats. Try the most common ones.
        var formats = new[]
        {
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:sszzz",
            "yyyy-MM-dd",
            "dd-MMM-yyyy",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "yyyy.MM.dd",
        };

        // Some WHOIS servers append extra info after the date — take only the date portion
        var clean = raw.Split(' ', '\t')[0].TrimEnd('.');

        if (DateTime.TryParseExact(clean, formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out var result))
        {
            return result;
        }

        // Fallback: let .NET try its own heuristics
        if (DateTime.TryParse(raw,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out var fallback))
        {
            return fallback;
        }

        return null;
    }
}
