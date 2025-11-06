using System.Net;
using System.Text.RegularExpressions;
using ASHelpers.Extensions;
using HeimdallWeb.Interfaces;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners;

public class SensitivePathsScanner : IScanner
{
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _connectTimeout;
    private readonly TimeSpan _readTimeout;
    private readonly int _maxParallel;

    /// <summary>
    /// Lista padrão de caminhos sensíveis a serem verificados.
    /// </summary>
    private readonly List<string> _defaultPaths = new()
    {
        "/admin","/admin/login","/administrator","/administrator/index.php","/wp-admin",
        "/wp-login.php","/xmlrpc.php","/wp-content","/wp-includes","/joomla/administrator",
        "/typo3","/typo3/install.php","/drupal","/user/login","/core/install.php","/phpinfo.php",
        "/info.php","/test.php","/setup.php","/install.php","/config.php","/.env","/.git","/.git/config",
        "/.htaccess","/.bash_history","/.ssh/authorized_keys","/.svn","/.DS_Store",
        "/.well-known/security.txt","/backup.zip","/backup.sql","/db.sql","/dump.sql","/adminer.php",
        "/pma","/phpmyadmin","/pmadb","/solr/admin","/server-status","/manager","/manager/html",
        "/console","/actuator","/actuator/health","/actuator/env","/WEB-INF/web.xml","/web.config",
        "/phpmyadmin/index.php","/phpmyadmin/scripts/setup.php",".gitignore",
    };

    /// <summary>
    /// Construtor do SensitivePathsScanner.
    /// </summary>
    /// <param name="connectTimeout"></param>
    /// <param name="readTimeout"></param>
    /// <param name="maxParallel"></param>
    public SensitivePathsScanner(TimeSpan? connectTimeout = null,
        TimeSpan? readTimeout = null, int maxParallel = 10)
    {
        _connectTimeout = connectTimeout ?? TimeSpan.FromSeconds(5);
        _readTimeout = readTimeout ?? TimeSpan.FromSeconds(8);
        _maxParallel = Math.Max(1, maxParallel);

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false
        };
        _httpClient = new HttpClient(handler);
        _httpClient.Timeout = TimeSpan.FromSeconds(10); // fallback geral

        // Add a default User-Agent (some servers reject requests without it)
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "HeimdallScanner/1.0 (+https://example)");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
    }

    public async Task<JObject> ScanAsync(string target, CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new JArray();
            var listToCheck = _defaultPaths;

            // Limita o número de tarefas paralelas
            using var sem = new SemaphoreSlim(_maxParallel);
            var tasks = listToCheck.Select(async path =>
            {
                await sem.WaitAsync();
                try
                {
                    var item = await ProbePathAsync(target, path);
                    // Only add positive findings to reduce JSON size
                    if (item is not null && item["exists"]?.Value<bool>() == true && (
                        item["evidence"]?.Value<string>() is not null ||
                        item["severity"]?.Value<string>() != "Baixo"))
                    {
                        lock (results)
                        {
                            results.Add(item);
                        }
                    }
                }
                finally
                {
                    sem.Release();
                }
            }).ToArray();

            await Task.WhenAll(tasks);

            return await Task.FromResult(new JObject
            {
                ["sensitivePathScanner"] = new JObject
                {
                    ["timestamp"] = DateTime.Now,
                    ["results"] = results
                }
            });
        }
        catch (Exception ex)
        {
            return await Task.FromResult(new { scanner = "SensitivePathsScanner", error = ex.Message }.ToJson());
        }

    }

    private async Task<JObject> ProbePathAsync(string target, string path)
    {
        var result = new JObject
        {
            ["path"] = path,
            ["exists"] = false,
            ["status_code"] = 0,
            ["redirectLocation"] = null,
            ["evidence"] = null,
            ["severity"] = "Baixo", // padrão 
            ["exception"] = null
        };

        try
        {
            var cts = new CancellationTokenSource(_connectTimeout);

            string requestUriString = $"{target}{path}";

            // Envio requisição HEAD
            var headReq = new HttpRequestMessage(HttpMethod.Head, requestUriString);
            var headTask = _httpClient.SendAsync(headReq, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            var completed = await Task.WhenAny(headTask, Task.Delay(_connectTimeout));

            if (completed != headTask || headTask.IsCanceled || headTask.IsFaulted)
            {
                // capture exception if any
                if (headTask.IsFaulted)
                {
                    result["exception"] = headTask.Exception?.GetBaseException().Message;
                }

                // fallback: muitos servidores bloqueiam HEAD -> tente um GET curto
                try
                {
                    using var ctsGet = new CancellationTokenSource(_connectTimeout);
                    var getReq = new HttpRequestMessage(HttpMethod.Get, requestUriString);
                    var getResp = await _httpClient.SendAsync(getReq, HttpCompletionOption.ResponseHeadersRead, ctsGet.Token);

                    result["status_code"] = (int)getResp.StatusCode;

                    if ((int)getResp.StatusCode >= 200 && (int)getResp.StatusCode < 300)
                    {
                        result["exists"] = true;
                        result["severity"] = DetermineSeverity(path, getResp.StatusCode, null);

                        if (ShouldGrabEvidence(path))
                        {
                            var evidence = await GrabEvidenceAsync(requestUriString, path);
                            if (!string.IsNullOrEmpty(evidence)) result["evidence"] = evidence;
                        }

                        return result;
                    }

                    if (getResp.StatusCode == HttpStatusCode.MovedPermanently || getResp.StatusCode == HttpStatusCode.Found || getResp.StatusCode == HttpStatusCode.TemporaryRedirect || getResp.StatusCode == HttpStatusCode.PermanentRedirect)
                    {
                        result["exists"] = true;
                        if (getResp.Headers.Location != null) result["redirectLocation"] = getResp.Headers.Location.ToString();
                        result["severity"] = DetermineSeverity(path, getResp.StatusCode, getResp.Headers.Location?.ToString());
                        return result;
                    }

                    if (getResp.StatusCode == HttpStatusCode.Forbidden)
                    {
                        result["exists"] = true;
                        result["severity"] = "Baixo";
                        return result;
                    }

                    if (getResp.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        result["exists"] = true;
                        result["severity"] = "Medio";
                        return result;
                    }

                    // otherwise keep exists=false and return
                    return result;
                }
                catch (Exception ex)
                {
                    result["exception"] = ex.Message;
                    result["evidence"] = ex.Message;
                    return result;
                }
            }

            var headResp = headTask.Result;
            result["status_code"] = (int)headResp.StatusCode;

            // if server explicitly disallows HEAD, fall back to GET
            if (headResp.StatusCode == HttpStatusCode.MethodNotAllowed || headResp.StatusCode == HttpStatusCode.NotImplemented)
            {
                try
                {
                    using var ctsGet2 = new CancellationTokenSource(_connectTimeout);
                    var getReq2 = new HttpRequestMessage(HttpMethod.Get, requestUriString);
                    var getResp2 = await _httpClient.SendAsync(getReq2, HttpCompletionOption.ResponseHeadersRead, ctsGet2.Token);

                    result["status_code"] = (int)getResp2.StatusCode;

                    if ((int)getResp2.StatusCode >= 200 && (int)getResp2.StatusCode < 300)
                    {
                        result["exists"] = true;
                        result["severity"] = DetermineSeverity(path, getResp2.StatusCode, null);

                        if (ShouldGrabEvidence(path))
                        {
                            var evidence = await GrabEvidenceAsync(requestUriString, path);
                            if (!string.IsNullOrEmpty(evidence)) result["evidence"] = evidence;
                        }

                        return result;
                    }

                    if (getResp2.StatusCode == HttpStatusCode.MovedPermanently || getResp2.StatusCode == HttpStatusCode.Found || getResp2.StatusCode == HttpStatusCode.TemporaryRedirect || getResp2.StatusCode == HttpStatusCode.PermanentRedirect)
                    {
                        result["exists"] = true;
                        if (getResp2.Headers.Location != null) result["redirectLocation"] = getResp2.Headers.Location.ToString();
                        result["severity"] = DetermineSeverity(path, getResp2.StatusCode, getResp2.Headers.Location?.ToString());
                        return result;
                    }

                    if (getResp2.StatusCode == HttpStatusCode.Forbidden)
                    {
                        result["exists"] = true;
                        result["severity"] = "Baixo";
                        return result;
                    }

                    if (getResp2.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        result["exists"] = true;
                        result["severity"] = "Medio";
                        return result;
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    result["exception"] = ex.Message;
                    result["evidence"] = ex.Message;
                    return result;
                }
            }

            // Replace cascading if/else-if with switch using pattern matching
            switch ((int)headResp.StatusCode)
            {
                case int sc when sc >= 200 && sc < 300:
                    result["exists"] = true;
                    result["severity"] = DetermineSeverity(path, headResp.StatusCode, null);
                    // tenta extrair pouca evidencia via GET (somente se for interessante)
                    if (ShouldGrabEvidence(path))
                    {
                        var evidence = await GrabEvidenceAsync(requestUriString, path);
                        if (!string.IsNullOrEmpty(evidence))
                        {
                            result["evidence"] = evidence;
                        }
                    }

                    return result;

                case 301:
                case 302:
                case 307:
                case 308:
                    result["exists"] = true;
                    if (headResp.Headers.Location != null)
                        result["redirectLocation"] = headResp.Headers.Location.ToString();
                    result["severity"] = DetermineSeverity(path, headResp.StatusCode, headResp.Headers.Location?.ToString());
                    break;

                case 403:
                    result["exists"] = true; // recurso existe, mas protegido
                    result["severity"] = "Baixo";
                    break;

                case 401:
                    result["exists"] = true;
                    result["severity"] = "Medio";
                    break;

                default:
                    result["exists"] = false;
                    break;
            }
        }
        catch (HttpRequestException hre)
        {
            result["exception"] = hre.Message;
            result["evidence"] = hre.Message;
            return result;
        }
        catch (TaskCanceledException)
        {
            result["exception"] = "timeout";
            result["evidence"] = "Houve um timeout na requisição";
            return result;
        }
        catch (Exception ex)
        {
            result["exception"] = ex.Message;
            result["evidence"] = string.Empty;
            return result;
        }

        return result;
    }

    /// <summary>
    /// Decide se merece um GET para extrair evidência (ex: phpinfo, admin login form)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool ShouldGrabEvidence(string path)
    {
        // paths que costumam retornar conteúdo útil para provar a existência
        var interesting = new[] 
        { 
            "/phpinfo.php", "/info.php", "/admin", 
            "/wp-login.php", "/wp-admin", "/phpmyadmin", 
            "/pma", "/adminer.php", "/.env", "/.git", "/config.php",
            "/backup.zip", "/backup.sql", "/db.sql", "/dump.sql", "/web.config",
            "/server-status", "/solr", "/actuator", "/manager/html",
            "/sitemap.xml", ".gitignore"
        };
        return interesting.Any(p => path.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// Faz GET com leitura limitada (não carrega body inteiro) e aplica heurísticas por path
    /// </summary>
    /// <param name="fullUri"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task<string?> GrabEvidenceAsync(string fullUri, string path)
    {
        try
        {
            using var cts = new CancellationTokenSource(_readTimeout);
            var req = new HttpRequestMessage(HttpMethod.Get, fullUri);
            var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!resp.IsSuccessStatusCode) return null;

            using var stream = await resp.Content.ReadAsStreamAsync(cts.Token);
            using var reader = new System.IO.StreamReader(stream);

            char[] buffer = new char[2048]; // lê até 2KB para heurísticas mais confiáveis

            int read = await reader.ReadAsync(buffer, 0, buffer.Length);

            if (read <= 0) return null;

            var snippet = new string(buffer, 0, Math.Min(read, buffer.Length));
            var sLower = snippet.ToLowerInvariant();

            // heurísticas por path
            if (path.IndexOf(".env", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (sLower.Contains("db_password") || sLower.Contains("database") || sLower.Contains("app_key") || sLower.Contains("mail_password"))
                    return "Conteúdo do .env foi vazado, contém dados sensíveis";
                return null; // don't return generic snippet for .env if nothing sensitive found
            }

            if (path.IndexOf(".git", StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf("gitignore", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (sLower.Contains("ref: refs/heads") || Regex.IsMatch(sLower, @"commit [0-9a-f]{7,}"))
                    return "Repositório .git descoberto";
                if (sLower.Contains(".gitignore") || sLower.Contains("node_modules"))
                    return "Metadados do .gitignore descobertos";
            }

            if (path.IndexOf("phpmyadmin", StringComparison.OrdinalIgnoreCase) >= 0 || path.IndexOf("pma", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (sLower.Contains("phpmyadmin") || sLower.Contains("pma") || sLower.Contains("phpmyadmin"))
                    return "Interface do phpMyAdmin";
            }

            if (path.IndexOf("phpinfo", StringComparison.OrdinalIgnoreCase) >= 0 || sLower.Contains("phpinfo()") || sLower.Contains("php version"))
            {
                return "Phpinfo detectado";
            }

            if (path.IndexOf("backup", StringComparison.OrdinalIgnoreCase) >= 0 || path.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            {
                if (sLower.Contains("insert into") || sLower.Contains("create table") || sLower.Contains("-- dump") || sLower.Contains("mysqldump"))
                    return "SQL dump / arquivo de backup detectado";
            }

            if (path.IndexOf("web.config", StringComparison.OrdinalIgnoreCase) >= 0 || sLower.Contains("<configuration>"))
            {
                return "Web.config ou ASP.NET conteúdo verificação";
            }

            if (path.IndexOf("server-status", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (sLower.Contains("server status") || sLower.Contains("server uptime") || sLower.Contains("apache"))
                    return "Página do status do servidor detectada";
            }

            if (path.IndexOf("solr", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (sLower.Contains("solr") || sLower.Contains("lucene") || sLower.Contains("solr admin"))
                    return "Solr interface do admin detectada";
            }

            if (path.IndexOf("actuator", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // actuator often returns JSON with status
                if (sLower.Contains("up") || sLower.Contains("status") || sLower.Contains("health"))
                    return "Endpoint do Spring Boot Actuator detectado";
            }

            // Generic heuristics similar to previous implementation
            if (sLower.Contains("wp-login.php") || sLower.Contains("wordpress"))
            {
                return "Wordpress login / admin panel detectado";
            }

            if (sLower.Contains("<title>"))
            {
                var start = snippet.IndexOf("<title>", StringComparison.OrdinalIgnoreCase);
                var end = snippet.IndexOf("</title>", StringComparison.OrdinalIgnoreCase);
                if (start >= 0 && end > start)
                {
                    return snippet.Substring(start, Math.Min(end + 8 - start, 200));
                }
            }

            // fallback small snippet (trim) but avoid returning large or binary content
            var safe = snippet.Length > 200 ? snippet.Substring(0, 200) : snippet;
            if (Regex.IsMatch(safe, "[<>]")) // likely HTML
                return safe;
                
            // if plain text but includes SQL keywords, secrets or config markers, surface it
            if (Regex.IsMatch(safe.ToLowerInvariant(), "(insert into|create table|password|db_pass|app_key|aws_access_key)"))
                return safe;

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///  Determinar a severidade com base no código HTTP e no path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="code"></param>
    /// <param name="redirect"></param>
    /// <returns></returns>
    private static string DetermineSeverity(string path, HttpStatusCode code, string? redirect)
    {
        // Regras simples (refine conforme necessidade)
        int sc = (int)code;

        if (sc >= 200 && sc < 300)
        {
            if (path.Contains("/backup") || path.EndsWith(".sql") || path.Contains(".env") || path.Contains(".git"))
                return "Critico";
            if (path.Contains("phpinfo") || path.Contains("phpmyadmin") || path.Contains("adminer"))
                return "Alto";
            if (path.Contains("/admin") || path.Contains("wp-login") || path.Contains("/manager"))
                return "Alto";
            if (path.Contains("/actuator") || path.Contains("/server-status") || path.Contains("/solr"))
                return "Medio";
            return "Medio";
        }

        if (sc == 301 || sc == 302)
        {
            if (!string.IsNullOrEmpty(redirect))
                return "Baixo";
        }

        if (sc == 403) return "Baixo";

        if (sc == 401) return "Medio";

        return "Baixo";
    }
}
