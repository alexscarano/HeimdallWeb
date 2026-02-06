using HeimdallWeb.Application.Helpers;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading;
using System.Net.Http;

namespace HeimdallWeb.Application.Services.Scanners
{
    public class HeaderScanner : IScanner
    {
        private readonly Dictionary<string, Func<string, bool>> _securityHeaders = new()
        {
            { "Strict-Transport-Security", v => v.Contains("max-age") },
            { "Content-Security-Policy", v => !string.IsNullOrWhiteSpace(v) },
            { "X-Frame-Options", v => v.Equals("DENY", StringComparison.OrdinalIgnoreCase) ||
                                      v.Equals("SAMEORIGIN", StringComparison.OrdinalIgnoreCase) },
            { "X-Content-Type-Options", v => v.Equals("nosniff", StringComparison.OrdinalIgnoreCase) },
            { "Referrer-Policy", v => !v.Equals("unsafe-url", StringComparison.OrdinalIgnoreCase) },
            { "Permissions-Policy", v => !string.IsNullOrWhiteSpace(v) },
            { "Cache-Control", v => v.Contains("no-store") || v.Contains("no-cache") },
        };

        private static readonly string[] SessionCookieNames = new[]
        {
            "ASP.NET_SessionId",
            "PHPSESSID",
            "JSESSIONID",
            "SID",
            "SESSIONID"
        };

        public async Task<JObject> ScanAsync(string target, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = new HttpClient();

                var response = await client.GetAsync(target, cancellationToken);
     
                var headers = response.Headers
                    .ToDictionary(h => h.Key, h => string.Join(";", h.Value));

                var contentHeaders = response.Content.Headers
                    .ToDictionary(h => h.Key, h => string.Join(";", h.Value));

                var allHeaders = headers.Concat(contentHeaders)
                                        .ToDictionary(h => h.Key, h => h.Value);

                Console.WriteLine("[HeaderScanner] Verificando headers HTTP");

                var present = new Dictionary<string, string>();
                var weak = new Dictionary<string, string>();
                var missing = new List<string>();

                foreach (var secHeader in _securityHeaders)
                {
                    if (headers.TryGetValue(secHeader.Key, out string keyValue))
                    {
                        if (secHeader.Value(keyValue))
                            present[secHeader.Key] = keyValue;
                        else
                            weak[secHeader.Key] = keyValue;
                    }
                    else
                    {
                        missing.Add(secHeader.Key);
                    }
                }

                var cookies = new JArray();
                if (response.Headers.TryGetValues("Set-Cookie", out var sessionCookies))
                {
                    Console.WriteLine("[HeaderScanner] Inspecionando cookies e flags de segurança");
                    foreach (var cookie in sessionCookies)
                    {
                        var cookieObj = AnalyzeCookie(cookie);
                        cookies.Add(cookieObj);
                    }
                }

                return JObject.FromObject(new
                {
                    statusCodeHttpRequest = (int)response.StatusCode,
                    headers = allHeaders,
                    securityHeaders = new
                    {
                        present,
                        weak,
                        missing
                    },
                    cookies = cookies,
                    scanTime = DateTime.Now
                });

            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return new JObject();
            }
        }

        private static JObject AnalyzeCookie(string cookieHeader)
        {
            // Separar por ';' e limpar espaços
            var parts = cookieHeader.Split(';')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            // nome=valor é o primeiro token (pode conter '=' no valor, então split apenas no primeiro)
            var first = parts.FirstOrDefault() ?? string.Empty;
            var idx = first.IndexOf('=');
            string name = idx >= 0 ? first.Substring(0, idx) : first;
            string value = idx >= 0 ? first.Substring(idx + 1) : string.Empty;

            bool hasSecure = parts.Any(p => p.Equals("secure", StringComparison.OrdinalIgnoreCase));
            bool hasHttpOnly = parts.Any(p => p.Equals("httponly", StringComparison.OrdinalIgnoreCase));

            // SameSite: pode ser "SameSite=None" ou 'samesite=none'
            var sameSitePart = parts.FirstOrDefault(p => p.StartsWith("samesite", StringComparison.OrdinalIgnoreCase));
            string sameSite = "Not set";
            if (sameSitePart is not null)
            {
                var eq = sameSitePart.IndexOf('=');
                sameSite = eq >= 0 ? sameSitePart.Substring(eq + 1).Trim() : sameSitePart;
                // normalize
                sameSite = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(sameSite.ToLowerInvariant());
            }

            // Domain
            var domainPart = parts.FirstOrDefault(p => p.StartsWith("domain=", StringComparison.OrdinalIgnoreCase));
            string domain = domainPart is not null ? domainPart.Substring(domainPart.IndexOf('=') + 1).Trim() : "";

            // Path
            var pathPart = parts.FirstOrDefault(p => p.StartsWith("path=", StringComparison.OrdinalIgnoreCase));
            string path = pathPart is not null ? pathPart.Substring(pathPart.IndexOf('=') + 1).Trim() : "";

            // Check prefixes
            bool hasSecurePrefix = name.StartsWith("__Secure-", StringComparison.OrdinalIgnoreCase);
            bool hasHostPrefix = name.StartsWith("__Host-", StringComparison.OrdinalIgnoreCase);

            // Detect cookie type (session-like)
            bool isSessionCookie = SessionCookieNames.Any(s => s.Equals(name, StringComparison.OrdinalIgnoreCase));

            // score risk
            string risk;
            string description = "";
            if (!hasHttpOnly && !hasSecure)
            {
                risk = "Critico";
                description = "Cookie sem HttpOnly e sem Secure — altamente suscetível a roubo por XSS e exposição em conexões não-HTTPS.";
            }
            else if (!hasHttpOnly)
            {
                risk = "Alto";
                description = "Cookie sem HttpOnly, pode ser acessado via JavaScript (risco de XSS).";
            }
            else if (!hasSecure)
            {
                risk = "Medio";
                description = "Cookie sem Secure — pode ser transmitido via HTTP não criptografado.";
            }
            else if (sameSite == "Not set" || string.IsNullOrEmpty(sameSite) || sameSite.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                // SameSite=None é aceitável se Secure presente; porém pode aumentar superfície
                risk = "Baixo";
                description = "SameSite não definido (ou None). Recomenda-se SameSite=Lax/Strict para mitigar CSRF quando aplicável.";
            }
            else
            {
                risk = "Informativo";
                description = "Cookie com flags de segurança adequadas (HttpOnly, Secure, SameSite).";
            }

            // Improve description with domain/path/prefix hints
            if (!string.IsNullOrEmpty(domain))
            {
                if (domain.StartsWith("."))
                    description += " Cookie com scoping amplo (Domain começa com '.') pode ser compartilhado entre subdomínios.";
            }
            if (!string.IsNullOrEmpty(path) && path == "/")
            {
                description += " Path='/' dá escopo global ao cookie.";
            }
            if (hasHostPrefix && !hasSecure)
            {
                description += " Cookie com prefixo __Host- sem Secure é inconsistente com boas práticas.";
            }

            return new JObject
            {
                ["nome"] = name,
                ["value_sample"] = value.Length <= 64 ? value : value.Substring(0, 64),
                ["temSecure"] = hasSecure,
                ["temHttpOnly"] = hasHttpOnly,
                ["sameSite"] = sameSite,
                ["domain"] = domain,
                ["path"] = path,
                ["prefix_secure"] = hasSecurePrefix,
                ["prefix_host"] = hasHostPrefix,
                ["isSessionCookie"] = isSessionCookie,
                ["risco"] = risk,
                ["descricao"] = description.Trim()
            };
        }
    }



}
