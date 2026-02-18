using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

public class SecurityTxtScanner : IScanner
{
    public ScannerMetadata Metadata => new(
        Key: "SecurityTxt",
        DisplayName: "security.txt (RFC 9116)",
        Category: "General",
        DefaultTimeout: TimeSpan.FromSeconds(8));

    private static readonly Regex ExpiresRegex = new(
        @"^Expires\s*:\s*(.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex ContactRegex = new(
        @"^Contact\s*:\s*(.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex EncryptionRegex = new(
        @"^Encryption\s*:\s*(.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

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

            var baseUrl = target.TrimEnd('/');

            // Primary location per RFC 9116
            var primaryUrl = $"{baseUrl}/.well-known/security.txt";
            // Legacy fallback
            var fallbackUrl = $"{baseUrl}/security.txt";

            cancellationToken.ThrowIfCancellationRequested();

            var (content, foundUrl) = await FetchSecurityTxtAsync(client, primaryUrl, fallbackUrl, cancellationToken);

            if (content is null)
            {
                return new JObject
                {
                    ["security_txt"] = new JObject
                    {
                        ["present"] = false,
                        ["alerts"] = new JArray { "security.txt not found (RFC 9116)" }
                    }
                };
            }

            bool hasContact = ContactRegex.IsMatch(content);
            bool hasEncryption = EncryptionRegex.IsMatch(content);

            bool isExpired = false;
            string? expiresIso = null;

            var expiresMatch = ExpiresRegex.Match(content);
            if (expiresMatch.Success)
            {
                var rawExpires = expiresMatch.Groups[1].Value.Trim();
                if (DateTime.TryParse(rawExpires,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out var expiresDate))
                {
                    isExpired = expiresDate < DateTime.UtcNow;
                    expiresIso = expiresDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
            }

            var alerts = new JArray();
            if (!hasContact)
                alerts.Add("security.txt is missing the mandatory Contact field (RFC 9116 §2.5.3)");
            if (isExpired)
                alerts.Add("security.txt has expired — update the Expires field");
            if (!hasEncryption)
                alerts.Add("security.txt does not include an Encryption field — consider adding a PGP key URL");

            return new JObject
            {
                ["security_txt"] = new JObject
                {
                    ["present"] = true,
                    ["url"] = foundUrl,
                    ["has_contact"] = hasContact,
                    ["has_encryption"] = hasEncryption,
                    ["is_expired"] = isExpired,
                    ["expires"] = expiresIso is not null ? (JToken)expiresIso : JValue.CreateNull(),
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
                ["security_txt"] = new JObject
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    private static async Task<(string? Content, string? Url)> FetchSecurityTxtAsync(
        HttpClient client,
        string primaryUrl,
        string fallbackUrl,
        CancellationToken ct)
    {
        foreach (var url in new[] { primaryUrl, fallbackUrl })
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var response = await client.GetAsync(url, ct);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    return (content, url);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // Try fallback
            }
        }

        return (null, null);
    }
}
