using System.Net;
using System.Net.Sockets;
using HeimdallWeb.Application.Helpers;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

public class IpChangeScanner : IScanner
{
    public ScannerMetadata Metadata => new(
        Key: "IpChange",
        DisplayName: "IP Resolution & CDN Detection",
        Category: "General",
        DefaultTimeout: TimeSpan.FromSeconds(8));

    // CDN CIDR ranges (IPv4 only — checked via simple uint arithmetic)
    private static readonly IReadOnlyList<(string Provider, uint NetworkAddress, uint Mask)> CdnRanges
        = BuildCdnRanges();

    public async Task<JObject> ScanAsync(string targetRaw, CancellationToken cancellationToken = default)
    {
        try
        {
            var hostname = NetworkUtils.RemoveHttpString(targetRaw);

            cancellationToken.ThrowIfCancellationRequested();

            IPAddress[] addresses = await Dns.GetHostAddressesAsync(hostname, cancellationToken);

            var ipv4List = new JArray();
            var ipv6List = new JArray();
            string? cdnProvider = null;

            foreach (var addr in addresses)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipv4List.Add(addr.ToString());
                    if (cdnProvider is null)
                        cdnProvider = DetectCdn(addr);
                }
                else if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    ipv6List.Add(addr.ToString());
                }
            }

            var alerts = new JArray();
            if (cdnProvider is not null)
                alerts.Add($"IP is behind a CDN ({cdnProvider}) — real origin server IP may be hidden");

            if (addresses.Length == 0)
                alerts.Add("DNS resolution returned no addresses");

            return new JObject
            {
                ["ip_resolution"] = new JObject
                {
                    ["hostname"] = hostname,
                    ["ipv4_addresses"] = ipv4List,
                    ["ipv6_addresses"] = ipv6List,
                    ["behind_cdn"] = cdnProvider is not null,
                    ["cdn_provider"] = cdnProvider is not null ? (JToken)cdnProvider : JValue.CreateNull(),
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
                ["ip_resolution"] = new JObject
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    private static string? DetectCdn(IPAddress addr)
    {
        if (addr.AddressFamily != AddressFamily.InterNetwork)
            return null;

        var bytes = addr.GetAddressBytes();
        uint ipUint = ((uint)bytes[0] << 24)
                    | ((uint)bytes[1] << 16)
                    | ((uint)bytes[2] << 8)
                    | bytes[3];

        foreach (var (provider, network, mask) in CdnRanges)
        {
            if ((ipUint & mask) == network)
                return provider;
        }

        return null;
    }

    private static List<(string, uint, uint)> BuildCdnRanges()
    {
        // Format: (provider, "x.x.x.x/prefix")
        var specs = new[]
        {
            // Cloudflare
            ("Cloudflare", "103.21.244.0/22"),
            ("Cloudflare", "103.22.200.0/22"),
            ("Cloudflare", "103.31.4.0/22"),
            ("Cloudflare", "104.16.0.0/13"), // Covers 104.16.0.0 - 104.23.255.255
            ("Cloudflare", "104.24.0.0/14"),
            ("Cloudflare", "108.162.192.0/18"),
            ("Cloudflare", "131.0.72.0/22"),
            ("Cloudflare", "141.101.64.0/18"),
            ("Cloudflare", "162.158.0.0/15"),
            ("Cloudflare", "172.64.0.0/13"),
            ("Cloudflare", "173.245.48.0/20"),
            ("Cloudflare", "188.114.96.0/20"),
            ("Cloudflare", "190.93.240.0/20"),
            ("Cloudflare", "197.234.240.0/22"),
            ("Cloudflare", "198.41.128.0/17"),
            // Fastly
            ("Fastly", "151.101.0.0/16"),
            // Akamai
            ("Akamai", "23.0.0.0/12"),
        };

        var list = new List<(string, uint, uint)>();

        foreach (var (provider, cidr) in specs)
        {
            var parts = cidr.Split('/');
            var ipParts = parts[0].Split('.');
            int prefix = int.Parse(parts[1]);

            uint network = ((uint)byte.Parse(ipParts[0]) << 24)
                         | ((uint)byte.Parse(ipParts[1]) << 16)
                         | ((uint)byte.Parse(ipParts[2]) << 8)
                         | byte.Parse(ipParts[3]);

            uint mask = prefix == 0 ? 0u : (0xFFFFFFFFu << (32 - prefix));
            list.Add((provider, network & mask, mask));
        }

        return list;
    }
}
