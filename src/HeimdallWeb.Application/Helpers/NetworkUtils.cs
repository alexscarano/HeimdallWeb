using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;

namespace HeimdallWeb.Application.Helpers;

public static class NetworkUtils
{
    /// <summary>
    /// Normalização de URL
    /// </summary>
    public static string NormalizeUrl(this string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("A url não pode estar vazia");
            }

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = $"https://{url}";
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("O alvo informado não é uma URL válida.");
            }

            var uriResultString = uriResult.GetLeftPart(UriPartial.Authority).TrimEnd('/');

            return uriResultString;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Validação de formato (sintaxe)
    /// </summary>
    public static bool IsValidUrl(string url, out Uri? uriResult)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validação de acessibilidade (rede)
    /// </summary>
    public static async Task<bool> IsReachableAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("A url não pode estar vazia");
        }

        try
        {
            using var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
            client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

            try
            {
                var headResponse = await client.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, url),
                    HttpCompletionOption.ResponseHeadersRead
                );

                if (headResponse.IsSuccessStatusCode ||
                    ((int)headResponse.StatusCode >= 300 && (int)headResponse.StatusCode < 400))
                {
                    return true;
                }
            }
            catch
            {
                // HEAD pode falhar, tentar GET
            }

            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            return response.IsSuccessStatusCode ||
                   ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400);
        }
        catch (TaskCanceledException)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Remove o http/https do início da URL
    /// </summary>
    public static string RemoveHttpString(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("A url não pode estar vazia");
        }

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            url = url.Replace("http://", "", StringComparison.OrdinalIgnoreCase);
        }
        else if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = url.Replace("https://", "", StringComparison.OrdinalIgnoreCase);
        }

        if (url.Contains('/'))
        {
            url = url.Substring(0, url.IndexOf('/'));
        }

        return url;
    }

    /// <summary>
    /// Capturar o endereço ipv4 com base no DNS
    /// </summary>
    public static IPAddress[] GetIPv4Addresses(string rawHost)
    {
        List<IPAddress> ipv4Addresses = new List<IPAddress>();

        try
        {
            var host = RemoveHttpString(rawHost);
            var addresses = Dns.GetHostAddresses(host);
            foreach (var addr in addresses)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipv4Addresses.Add(addr);
                    return ipv4Addresses.ToArray();
                }
            }
            throw new Exception("Nenhum ipv4 foi achado para esse host.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro em resolver o '{rawHost}': {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica se a string é um endereço IP válido
    /// </summary>
    public static bool IsIPAddress(string input)
    {
        return IPAddress.TryParse(input, out _);
    }

    public static string? GetRemoteIPv4(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return null;
        }

        try
        {
            var headers = httpContext.Request?.Headers;
            if (headers is not null)
            {
                if (headers.TryGetValue("X-Forwarded-For", out var xff))
                {
                    var first = xff.ToString().Split(',').FirstOrDefault()?.Trim();
                    if (!string.IsNullOrEmpty(first) && IPAddress.TryParse(first, out var parsed))
                    {
                        if (parsed.AddressFamily == AddressFamily.InterNetwork)
                            return parsed.ToString();
                        if (parsed.AddressFamily == AddressFamily.InterNetworkV6 && parsed.IsIPv4MappedToIPv6)
                            return parsed.MapToIPv4().ToString();
                        return parsed.ToString();
                    }
                }

                if (headers.TryGetValue("X-Real-IP", out var xrip))
                {
                    var val = xrip.ToString().Trim();
                    if (!string.IsNullOrEmpty(val) && IPAddress.TryParse(val, out var parsed2))
                    {
                        if (parsed2.AddressFamily == AddressFamily.InterNetwork)
                            return parsed2.ToString();
                        if (parsed2.AddressFamily == AddressFamily.InterNetworkV6 && parsed2.IsIPv4MappedToIPv6)
                            return parsed2.MapToIPv4().ToString();
                        return parsed2.ToString();
                    }
                }
            }
        }
        catch
        {
            // Fall through
        }

        var ip = httpContext?.Connection?.RemoteIpAddress;
        if (ip is null)
        {
            return null;
        }

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            try
            {
                return ip.MapToIPv4().ToString();
            }
            catch
            {
            }
        }

        return ip.ToString();
    }

    /// <summary>
    /// Return IPv4 string or provided fallback (default empty string).
    /// </summary>
    public static string GetRemoteIPv4OrFallback(HttpContext? httpContext, string fallback = "")
    {
        return GetRemoteIPv4(httpContext) ?? fallback;
    }
}
