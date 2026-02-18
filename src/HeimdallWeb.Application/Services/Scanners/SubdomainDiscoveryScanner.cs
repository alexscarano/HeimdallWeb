using System.Net;
using System.Net.Sockets;
using HeimdallWeb.Application.Helpers;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

public class SubdomainDiscoveryScanner : IScanner
{
    public ScannerMetadata Metadata => new(
        Key: "Subdomain",
        DisplayName: "Subdomain Discovery",
        Category: "General",
        DefaultTimeout: TimeSpan.FromSeconds(15));

    private static readonly string[] CommonSubdomains =
    {
        "www", "api", "dev", "staging", "admin", "mail", "ftp", "vpn", "portal"
    };

    public async Task<JObject> ScanAsync(string targetRaw, CancellationToken cancellationToken = default)
    {
        try
        {
            var hostname = NetworkUtils.RemoveHttpString(targetRaw);

            // Strip any existing subdomain to get base domain (e.g. sub.example.com -> example.com)
            var baseDomain = ExtractBaseDomain(hostname);

            cancellationToken.ThrowIfCancellationRequested();

            var probeTasks = CommonSubdomains.Select(sub =>
                ProbeSubdomainAsync($"{sub}.{baseDomain}", cancellationToken));

            var probeResults = await Task.WhenAll(probeTasks);

            var discovered = new JArray();
            foreach (var (subdomain, ips) in probeResults.Where(r => r.Ips is not null))
            {
                var ipArray = new JArray(ips!.Select(ip => ip.ToString()));
                discovered.Add(new JObject
                {
                    ["subdomain"] = subdomain,
                    ["ips"] = ipArray
                });
            }

            var alerts = new JArray();
            if (discovered.Count > 0)
            {
                var sensitiveSubdomains = new[] { "admin", "dev", "staging", "vpn" };
                foreach (var entry in discovered.OfType<JObject>())
                {
                    var sub = entry["subdomain"]?.ToString() ?? string.Empty;
                    var label = sub.Split('.')[0];
                    if (sensitiveSubdomains.Contains(label, StringComparer.OrdinalIgnoreCase))
                        alerts.Add($"Sensitive subdomain found: {sub}");
                }
            }

            return new JObject
            {
                ["subdomains"] = new JObject
                {
                    ["base_domain"] = baseDomain,
                    ["discovered"] = discovered,
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
                ["subdomains"] = new JObject
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    private static async Task<(string Subdomain, IPAddress[]? Ips)> ProbeSubdomainAsync(
        string fqdn,
        CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            var addresses = await Dns.GetHostAddressesAsync(fqdn, ct);
            var ipv4 = addresses
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .ToArray();

            // Return all addresses if no IPv4 exists but IPv6 does
            var result = ipv4.Length > 0 ? ipv4 : addresses;
            return result.Length > 0 ? (fqdn, result) : (fqdn, null);
        }
        catch
        {
            return (fqdn, null);
        }
    }

    /// <summary>
    /// Extracts the registrable domain from a hostname.
    /// e.g. "sub.example.com" -> "example.com", "example.com" -> "example.com"
    /// Handles simple TLD (one dot) and common two-part TLDs (.co.uk, .com.br, etc.).
    /// </summary>
    private static string ExtractBaseDomain(string hostname)
    {
        var parts = hostname.Split('.');
        if (parts.Length <= 2)
            return hostname;

        // Heuristic: known two-part TLDs commonly end in short second-level labels
        var knownTwoPartTld = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "co.uk", "org.uk", "me.uk", "net.uk", "com.au", "net.au", "org.au",
            "com.br", "net.br", "org.br", "com.ar", "net.ar",
            "co.jp", "ne.jp", "or.jp", "ac.jp",
            "co.nz", "org.nz", "net.nz",
        };

        if (parts.Length >= 3)
        {
            var possibleTwoPartTld = $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}";
            if (knownTwoPartTld.Contains(possibleTwoPartTld))
                return $"{parts[parts.Length - 3]}.{possibleTwoPartTld}";
        }

        // Default: last two parts
        return $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}";
    }
}
