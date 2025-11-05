using HeimdallWeb.Interfaces;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners;

public class RobotsScanner : IScanner
{
    public async Task<JObject> ScanAsync(string target, CancellationToken cancellationToken = default)
    {
        var alerts = new List<string>();

        string robotsUrl = target;
        string sitemapUrl = target;

        if (!target.Contains("/robots.txt"))
        {
            int idx = target.IndexOf("sitemap.xml"); // Skip "http://" or "https://"
            robotsUrl = robotsUrl.Substring(0, idx >= 0 ? idx : target.Length);
            robotsUrl = $"{robotsUrl.TrimEnd('/')}/robots.txt";
        }
        else if (!target.Contains("/sitemap.xml"))
        {
            int idx = target.IndexOf("robots.txt"); // Skip "http://" or "https://"
            sitemapUrl = sitemapUrl.Substring(0, idx >= 0 ? idx : target.Length);
            sitemapUrl = $"{sitemapUrl.TrimEnd('/')}/sitemap.xml";
        }

        bool robotsFound = false;
        bool sitemapFound = false;
        string? robotsContent = null;
        string? foundSitemapUrl = null;

        using var client =  new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(25);

        try
        {
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

                int maxLines = 50;
                var lines = new List<string>();

                for (int i = 0; i < maxLines && !reader.EndOfStream; i++)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    lines.Add(line.Trim());
                }

                foreach (var line in lines)
                {
                    var idx = line.IndexOf("Sitemap:", StringComparison.OrdinalIgnoreCase);

                    if (idx == -1)
                        break;

                    if (idx >= 0)
                    {
                        foundSitemapUrl = line[(idx + 8)..].Trim(); 
                        sitemapFound = true;
                        break; 
                    }
                }

                robotsFound = true;
                robotsContent = string.Join('\n', lines);

                if (robotsContent.Contains("Disallow: /admin", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("O arquivo robots.txt expõe o diretório /admin.");
                }
                if (robotsContent.Contains("Disallow: /", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("O arquivo robots.txt contém 'Disallow: /', bloqueando todos os rastreadores.");
                }
                if (robotsContent.Contains("Crawl-delay", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("O arquivo robots.txt utiliza a diretiva Crawl-delay, que pode afetar o desempenho do rastreamento.");
                }
                if (robotsContent.Contains("User-agent: *", StringComparison.OrdinalIgnoreCase) &&
                    robotsContent.Contains("Disallow:", StringComparison.OrdinalIgnoreCase) &&
                    !robotsContent.Contains("Allow:", StringComparison.OrdinalIgnoreCase))
                {
                    alerts.Add("O arquivo robots.txt bloqueia todos os rastreadores sem exceções.");
                }
                if (robotsContent.Length < 30)
                {
                    alerts.Add("O arquivo robots.txt é muito pequeno, o que pode indicar uma configuração inadequada.");
                }
                if (robotsContent.Length > 10000)
                {
                    alerts.Add("O arquivo robots.txt é muito grande, o que pode indicar uma configuração complexa.");
                }
            }
            else
            {
                alerts.Add("O arquivo robots.txt não foi encontrado.");
            }
        }
        catch
        {
            alerts.Add("Não foi possível acessar o arquivo robots.txt.");
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
                alerts.Add("O sitemap.xml referenciado no robots.txt não foi encontrado.");
            }
        }

        return alerts.Count > 0 ? new JObject
        {
            ["robots"] = new JObject
            {
                ["robots_found"] = robotsFound,
                ["sitemap_found"] = sitemapFound,
                ["sitemap_url"] = foundSitemapUrl ?? string.Empty,
                ["alerts"] = JArray.FromObject(alerts)
            }
        }
        : new JObject
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
}
