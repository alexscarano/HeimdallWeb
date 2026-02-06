using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners;

public class RobotsScanner : IScanner
{
    public async Task<JObject> ScanAsync(string target, CancellationToken cancellationToken = default)
    {
        var alerts = new List<string>();

        string robotsUrl = target;
        string sitemapUrl = target;

        robotsUrl = $"{robotsUrl}/robots.txt";
        sitemapUrl = $"{sitemapUrl}/sitemap.xml";

        bool robotsFound = false;
        bool sitemapFound = false;
        string? robotsContent = null;
        string? foundSitemapUrl = null;

        using var client =  new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(25);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var robotsResponse = await client.GetAsync(robotsUrl, cancellationToken);

            if (!robotsResponse.Content.Headers.ContentType?.MediaType?.Contains("text/plain") ?? true)
            {
                alerts.Add("O robots.txt retornou um conteúdo inesperado (provavelmente HTML).");
                return new JObject
                {
                    ["robots"] = new JObject
                    {
                        ["robots_found"] = false,
                        ["sitemap_found"] = false,
                        ["sitemap_url"] = string.Empty,
                        ["alerts"] = JArray.FromObject(alerts)
                    }
                };
            }

            if (robotsResponse.IsSuccessStatusCode)
            {
                using var stream = await robotsResponse.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);

                var sb = new StringBuilder();
                int maxLines = 200;

                for (int i = 0; i < maxLines && !reader.EndOfStream; i++)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    sb.AppendLine(line.Trim());
                }

                string[] lines = sb.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var line in lines)
                {
                    if (line.StartsWith("Sitemap:", StringComparison.OrdinalIgnoreCase))
                    {
                        foundSitemapUrl = line.Split(':', 2)[1].Trim();
                        sitemapFound = true;
                        break;
                    }
                }


                robotsFound = true;
                robotsContent = string.Join('\n', lines);

                if (robotsContent.Contains("Disallow: /admin", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("Alto|O arquivo robots.txt expõe o diretório /admin");
                }
                if (robotsContent.Contains("Disallow: /", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("Medio|O arquivo robots.txt contém 'Disallow: /', bloqueando todos os rastreadores");
                }
                if (robotsContent.Contains("Crawl-delay", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("Informativo|O arquivo robots.txt utiliza a diretiva Crawl-delay");
                }
                if (robotsContent.Contains("User-agent: *", StringComparison.OrdinalIgnoreCase) &&
                    robotsContent.Contains("Disallow:", StringComparison.OrdinalIgnoreCase) &&
                    !robotsContent.Contains("Allow:", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("Baixo|O arquivo robots.txt bloqueia todos os rastreadores sem exceções");
                }
                if (robotsContent.Length < 30)
                {
                    alerts.Add("Informativo|O arquivo robots.txt é muito pequeno, pode indicar configuração inadequada");
                }
                if (robotsContent.Length > 10000)
                {
                    alerts.Add("Informativo|O arquivo robots.txt é muito grande, pode indicar configuração complexa");
                }

                // Additional heuristic checks
                // WordPress specific
                if (robotsContent.Contains("wp-admin", StringComparison.OrdinalIgnoreCase) || robotsContent.Contains("wp-login.php", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("Medio|O arquivo robots.txt referencia áreas do WordPress (wp-admin/wp-login)");
                }

                // Allow everything explicitly
                if (robotsContent.Contains("Allow: /", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("Informativo|O arquivo robots.txt contém 'Allow: /' permitindo acesso total");
                }

                // Sensitive paths referenced
                if (robotsContent.IndexOf("/backup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    robotsContent.IndexOf(".sql", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    robotsContent.IndexOf(".env", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    robotsContent.IndexOf("/dump", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    alerts.Add("Alto|O arquivo robots.txt referencia possíveis backups ou arquivos sensíveis");
                }

                // Host directive (Yandex/others)
                if (robotsContent.Contains("Host:", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("Informativo|O arquivo robots.txt contém a diretiva 'Host:'");
                }

                // Multiple sitemap entries
                var sitemapCount = lines.Count(l => l.StartsWith("Sitemap:", StringComparison.OrdinalIgnoreCase));
                if (sitemapCount > 1)
                {
                    alerts.Add($"Informativo|O arquivo robots.txt declara múltiplos sitemaps ({sitemapCount})");
                }

                // Crawl-delay value analysis
                var cdMatch = Regex.Match(robotsContent, @"Crawl-delay\s*:\s*(\d+)", RegexOptions.IgnoreCase);
                if (cdMatch.Success && int.TryParse(cdMatch.Groups[1].Value, out var cdVal))
                {
                    if (cdVal > 10)
                        alerts.Add($"Medio|Crawl-delay alto detectado ({cdVal}), pode reduzir a cobertura de rastreamento");
                    else if (cdVal > 3)
                        alerts.Add($"Informativo|Crawl-delay definido ({cdVal})");
                }

                // URLs with query strings in robots (could expose dynamic endpoints)
                if (robotsContent.Contains("?", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("Baixo|O arquivo robots.txt contém URLs com query string");
                }
            }
            else
            {
                alerts.Add("O arquivo robots.txt não foi encontrado.");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            alerts.Add("Baixo|Não foi possível acessar o arquivo robots.txt");
        }

        if (foundSitemapUrl is not null)
        {
            try
            {
                var siteMapResponse = await client.GetAsync(sitemapUrl, cancellationToken);
                if (siteMapResponse.IsSuccessStatusCode)
                {
                    sitemapFound = true;
                    foundSitemapUrl = sitemapUrl;
                }
            }
            catch
            {
                alerts.Add("Baixo|O sitemap.xml referenciado no robots.txt não foi encontrado");
            }
        }

        // Processa alerts para separar severity e message
        var processedAlerts = new JArray();
        foreach (var alert in alerts)
        {
            var parts = alert.Split('|', 2);
            if (parts.Length == 2)
            {
                processedAlerts.Add(new JObject
                {
                    ["severity"] = parts[0],
                    ["message"] = parts[1]
                });
            }
            else
            {
                processedAlerts.Add(new JObject
                {
                    ["severity"] = "Informativo",
                    ["message"] = alert
                });
            }
        }

        return alerts.Count >= 1 ? new JObject
        {
            ["robotsScanner"] = new JObject
            {
                ["robots_found"] = robotsFound,
                ["sitemap_found"] = sitemapFound,
                ["sitemap_url"] = foundSitemapUrl ?? string.Empty,
                ["alerts"] = processedAlerts
            }
        }
        : new JObject
        {
            ["robotsScanner"] = new JObject
            {
                ["robots_found"] = false,
                ["sitemap_found"] = false,
                ["sitemap_url"] = string.Empty,
                ["alerts"] = JArray.FromObject(alerts)
            }
        };

    }
}
