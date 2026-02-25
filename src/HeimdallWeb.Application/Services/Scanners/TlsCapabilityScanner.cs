using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using HeimdallWeb.Application.Helpers;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

public class TlsCapabilityScanner : IScanner
{
    public ScannerMetadata Metadata => new(
        Key: "TLS",
        DisplayName: "TLS Capability",
        Category: "SSL",
        DefaultTimeout: TimeSpan.FromSeconds(30));

    private static readonly HashSet<string> WeakCipherKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "RC4", "DES", "3DES", "NULL", "EXPORT", "anon"
    };

    public async Task<JObject> ScanAsync(string targetRaw, CancellationToken cancellationToken = default)
    {
        try
        {
            var hostname = NetworkUtils.RemoveHttpString(targetRaw);

            // Timeout for the entire scan operation
            using var scanCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            scanCts.CancelAfter(TimeSpan.FromSeconds(25));

            var (tls12Supported, tls12Cipher, tls12Error) = await TryNegotiateAsync(hostname, SslProtocols.Tls12, scanCts.Token);
            var (tls13Supported, tls13Cipher, tls13Error) = await TryNegotiateAsync(hostname, SslProtocols.Tls13, scanCts.Token);

            var negotiatedCipher = tls13Cipher ?? tls12Cipher ?? string.Empty;
            bool weakCipherDetected = IsWeakCipher(negotiatedCipher);

            var alerts = new JArray();
            if (!tls12Supported && !tls13Supported)
            {
                alerts.Add("Nenhuma versão TLS suportada pôde ser negociada na porta 443");
                if (!string.IsNullOrEmpty(tls12Error)) alerts.Add($"Erro TLS 1.2: {tls12Error}");
                if (!string.IsNullOrEmpty(tls13Error)) alerts.Add($"Erro TLS 1.3: {tls13Error}");
            }

            if (!tls13Supported && tls12Supported)
                alerts.Add("TLS 1.3 não é suportado — considere ativá-lo para maior segurança");

            if (weakCipherDetected)
                alerts.Add($"Cifra fraca detectada: {negotiatedCipher}");

            return new JObject
            {
                ["tls_capability"] = new JObject
                {
                    ["tls12_supported"] = tls12Supported,
                    ["tls13_supported"] = tls13Supported,
                    ["negotiated_cipher"] = negotiatedCipher,
                    ["weak_cipher_detected"] = weakCipherDetected,
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
                ["tls_capability"] = new JObject
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    private static async Task<(bool success, string? cipher, string? error)> TryNegotiateAsync(
        string hostname,
        SslProtocols protocol,
        CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            using var tcp = new TcpClient();
            try
            {
                await tcp.ConnectAsync(hostname, 443, cts.Token);
            }
            catch (Exception ex)
            {
                return (false, null, $"Falha de conexão: {ex.Message}");
            }

            // Fix: Do not pass the RemoteCertificateValidationCallback here because we are
            // already passing it in the SslClientAuthenticationOptions below.
            using var ssl = new SslStream(tcp.GetStream(), false);

            var authOptions = new SslClientAuthenticationOptions
            {
                TargetHost = hostname,
                EnabledSslProtocols = protocol,
                RemoteCertificateValidationCallback = (_, _, _, _) => true,
                EncryptionPolicy = EncryptionPolicy.RequireEncryption
            };

            await ssl.AuthenticateAsClientAsync(authOptions, cts.Token);

            var cipher = ssl.NegotiatedCipherSuite.ToString();
            return (true, cipher, null);
        }
        catch (AuthenticationException ex)
        {
            return (false, null, $"Falha de handshake: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, null, $"Erro: {ex.Message}");
        }
    }

    private static bool IsWeakCipher(string? cipher)
    {
        if (string.IsNullOrEmpty(cipher))
            return false;

        foreach (var keyword in WeakCipherKeywords)
        {
            if (cipher.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
