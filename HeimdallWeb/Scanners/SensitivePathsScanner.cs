using System.Net;
using System.Text.RegularExpressions;
using ASHelpers.Extensions;
using HeimdallWeb.Interfaces;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners;

/// <summary>
/// Scanner refatorado para detectar caminhos sensíveis sem gerar falsos-positivos.
/// Implementa heurísticas inteligentes para diferenciar páginas reais de erros personalizados.
/// </summary>
public class SensitivePathsScanner : IScanner
{
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _connectTimeout;
    private readonly TimeSpan _readTimeout;
    private readonly int _maxParallel;
    
    // Cache do conteúdo da homepage para comparação
    private string? _homepageContent;
    private int _homepageContentLength;
    
    // Dados para detecção de fallback global
    private class ProbeResult
    {
        public string Path { get; set; } = string.Empty;
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; } = string.Empty;
        public int ContentLength { get; set; }
        public bool IsPositiveFinding { get; set; }
    }

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
            Console.WriteLine("[SensitivePathsScanner] Iniciando varredura de caminhos sensíveis");
            
            // PASSO 1: Capturar conteúdo da homepage para comparação
            await CaptureHomepageBaselineAsync(target, cancellationToken);
            
            var allProbeResults = new List<ProbeResult>(); // 🟧 Rastreia TODAS as respostas
            var listToCheck = _defaultPaths;

            // Limita o número de tarefas paralelas
            using var sem = new SemaphoreSlim(_maxParallel);
            var tasks = listToCheck.Select(async path =>
            {
                await sem.WaitAsync(cancellationToken);
                try
                {
                    // Coleta dados SEM aplicar heurísticas ainda
                    var probeData = await ProbePathRawAsync(target, path, cancellationToken);
                    
                    if (probeData != null)
                    {
                        lock (allProbeResults)
                        {
                            allProbeResults.Add(probeData);
                        }
                    }
                }
                finally
                {
                    sem.Release();
                }
            }).ToArray();

            await Task.WhenAll(tasks);
            
            var http200Count = allProbeResults.Count(r => r.StatusCode == HttpStatusCode.OK);
            
            var fallbackDetection = DetectGlobalFallback(allProbeResults);
            
            if (fallbackDetection.IsSuspected)
            {
                Console.WriteLine($"[SensitivePathsScanner] Todos os caminhos retornaram 200 — possível falso-positivo. Razão: {fallbackDetection.Reason}");
                
                // Retorna um achado especial indicando fallback global detectado
                var fallbackFinding = new JObject
                {
                    ["path"] = "/*",
                    ["exists"] = true,
                    ["statusCode"] = 200,
                    ["severity"] = "Informativo",
                    ["evidence"] = $"Fallback global detectado: {fallbackDetection.Reason}",
                    ["type"] = "global-fallback",
                    ["description"] = "Todos os caminhos testados retornaram HTTP 200 com conteúdo similar, sugerindo roteamento catch-all ou configuração SPA. Caminhos individuais não foram validados como vulnerabilidades reais.",
                    ["diagnostics"] = new JObject
                    {
                        ["http200Count"] = fallbackDetection.Http200Count,
                        ["http200Percentage"] = Math.Round(fallbackDetection.Http200Percentage, 2),
                        ["avgContentSimilarity"] = Math.Round(fallbackDetection.AvgContentSimilarity, 4),
                        ["similarToHomepage"] = fallbackDetection.SimilarToHomepage,
                        ["testedPaths"] = listToCheck.Count
                    }
                };
                
                var fallbackResults = new JArray { fallbackFinding };
                
                return await Task.FromResult(new JObject
                {
                    ["sensitivePathScanner"] = new JObject
                    {
                        ["status"] = "suspected-fallback",
                        ["timestamp"] = DateTime.Now,
                        ["totalChecked"] = listToCheck.Count,
                        ["findings"] = 1,
                        ["results"] = fallbackResults
                    }
                });
            }
            
            //  PASSO 3: Se não há fallback global, aplica heurísticas individuais
            var results = new JArray();
            foreach (var probeData in allProbeResults)
            {
                var finding = await ApplyHeuristicsAsync(probeData, target, cancellationToken);
                if (finding != null && finding["_validFinding"]?.Value<bool>() == true)
                {
                    results.Add(finding);
                }
            }

            return await Task.FromResult(new JObject
            {
                ["sensitivePathScanner"] = new JObject
                {
                    ["timestamp"] = DateTime.Now,
                    ["totalChecked"] = listToCheck.Count,
                    ["findings"] = results.Count,
                    ["results"] = results
                }
            });
        }
        catch (Exception ex)
        {
            return await Task.FromResult(new { scanner = "SensitivePathsScanner", error = ex.Message }.ToJson());
        }
    }
    
    /// <summary>
    /// Classe auxiliar para resultado da detecção de fallback global
    /// </summary>
    private class FallbackDetectionResult
    {
        public bool IsSuspected { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int Http200Count { get; set; }
        public double Http200Percentage { get; set; }
        public double AvgContentSimilarity { get; set; }
        public bool SimilarToHomepage { get; set; }
    }
    
    /// <summary>
    /// HEURÍSTICA GLOBAL: Detecta quando todos os caminhos retornam 200 (fallback global)
    /// 
    /// Cenários detectados:
    /// 1. Single Page Applications (SPA) - todos caminhos retornam a mesma página
    /// 2. Custom error pages que respondem 200 ao invés de 404
    /// 3. URL rewriting genérico (IIS, Apache, Nginx)
    /// 
    /// Critérios:
    /// - 90%+ das respostas são HTTP 200
    /// - Conteúdo das respostas é muito similar entre si (>85% Jaccard)
    /// - OU conteúdo das respostas é muito similar à homepage
    /// </summary>
    private FallbackDetectionResult DetectGlobalFallback(List<ProbeResult> allProbeResults)
    {
        var result = new FallbackDetectionResult();
        
        
        if (allProbeResults.Count == 0)
        {
            return result; // Sem dados para analisar
        }
        
        // Conta quantas respostas retornaram 200
        var http200Results = allProbeResults.Where(r => r.StatusCode == HttpStatusCode.OK).ToList();
        var http200Count = http200Results.Count;
        var http200Percentage = (double)http200Count / allProbeResults.Count * 100;
        
        result.Http200Count = http200Count;
        result.Http200Percentage = http200Percentage;
        
        // Critério 1: Se menos de 90% retornou 200, não é fallback global
        if (http200Percentage < 90.0)
        {
            return result;
        }
        
        // Verifica se o conteúdo é similar à homepage
        if (!string.IsNullOrEmpty(_homepageContent) && http200Results.Any())
        {
            var homepageSimilarities = http200Results
                .Where(r => !string.IsNullOrEmpty(r.Content))
                .Select(r => CalculateSimilarity(r.Content, _homepageContent))
                .ToList();
            
            if (homepageSimilarities.Any())
            {
                var avgHomepageSimilarity = homepageSimilarities.Average();
                result.AvgContentSimilarity = avgHomepageSimilarity;
                result.SimilarToHomepage = avgHomepageSimilarity > 0.70; // Reduzido de 0.85 para 0.70
                
                if (avgHomepageSimilarity > 0.70) // Reduzido de 0.85 para 0.70
                {
                    result.IsSuspected = true;
                    result.Reason = $"Todos os caminhos sensíveis retornaram 200 com conteúdo similar à homepage (média de {avgHomepageSimilarity:P0} de similaridade). Isso sugere fallback global ou roteamento SPA.";
                    return result;
                }
            }
        }
        
        // Verifica se as respostas são similares entre si
        if (http200Results.Count >= 3)
        {
            var pairwiseSimilarities = new List<double>();
            
            // Compara pares de respostas (amostra aleatória para performance)
            var sampleSize = Math.Min(10, http200Results.Count);
            var samples = http200Results.Take(sampleSize).ToList();
            
            for (int i = 0; i < samples.Count; i++)
            {
                for (int j = i + 1; j < samples.Count; j++)
                {
                    if (!string.IsNullOrEmpty(samples[i].Content) && !string.IsNullOrEmpty(samples[j].Content))
                    {
                        var similarity = CalculateSimilarity(samples[i].Content, samples[j].Content);
                        pairwiseSimilarities.Add(similarity);
                    }
                }
            }
            
            if (pairwiseSimilarities.Any())
            {
                var avgPairwiseSimilarity = pairwiseSimilarities.Average();
                result.AvgContentSimilarity = avgPairwiseSimilarity;
                
                if (avgPairwiseSimilarity > 0.70) // Reduzido de 0.85 para 0.70
                {
                    result.IsSuspected = true;
                    result.Reason = $"Todos os caminhos sensíveis retornaram 200 com conteúdo similar (média de {avgPairwiseSimilarity:P0} de similaridade entre respostas). Isso sugere página de fallback global ou roteamento catch-all.";
                    return result;
                }
            }
        }
        
        // Verifica heurística de tamanho (todos retornam tamanho similar ±10%)
        if (http200Results.Count >= 3)
        {
            var contentLengths = http200Results.Select(r => r.ContentLength).Where(l => l > 0).ToList();
            
            if (contentLengths.Any())
            {
                var avgLength = contentLengths.Average();
                var maxDeviation = contentLengths.Select(l => Math.Abs(l - avgLength) / avgLength).Max();
                
                if (maxDeviation < 0.10) // Variação menor que 10%
                {
                    result.IsSuspected = true;
                    result.Reason = $"Todos os caminhos sensíveis retornaram 200 com tamanho de conteúdo quase idêntico (média de {avgLength:F0} bytes, desvio máximo de {maxDeviation:P0}). Isso sugere página de fallback genérica.";
                    return result;
                }
            }
        }
        
        // NOVO: Heurística de padrão de texto comum
        // Se 90%+ retornam 200 e todos compartilham uma frase comum significativa, é fallback
        if (http200Results.Count >= 5)
        {
            // Extrai a primeira frase significativa (>20 chars) do primeiro resultado
            var firstContent = http200Results.First().Content;
            if (!string.IsNullOrEmpty(firstContent) && firstContent.Length > 20)
            {
                var firstWords = firstContent.Split(' ').Take(5).ToArray();
                if (firstWords.Length >= 5)
                {
                    var commonPhrase = string.Join(" ", firstWords);
                    
                    // Verifica quantos resultados contêm essa frase
                    var matchCount = http200Results.Count(r => 
                        !string.IsNullOrEmpty(r.Content) && 
                        r.Content.Contains(commonPhrase, StringComparison.OrdinalIgnoreCase));
                    
                    var matchPercentage = (double)matchCount / http200Results.Count * 100;
                    
                    if (matchPercentage > 80) // 80%+ contêm a mesma frase inicial
                    {
                        result.IsSuspected = true;
                        result.Reason = $"Todos os caminhos sensíveis retornaram 200 e {matchPercentage:F0}% contêm o mesmo texto base (\"{commonPhrase.Substring(0, Math.Min(50, commonPhrase.Length))}...\"). Isso sugere fallback global ou página genérica.";
                        return result;
                    }
                }
            }
        }
        
        // FINAL: Se 95%+ retornaram HTTP 200, assume fallback mesmo sem análise de conteúdo
        // Isso captura casos extremos onde TODOS os caminhos retornam 200
        if (http200Percentage >= 95.0 && http200Results.Count >= 10)
        {
            result.IsSuspected = true;
            result.Reason = $"Taxa extremamente alta de HTTP 200: {http200Percentage:F1}% dos caminhos testados ({http200Count}/{allProbeResults.Count}). Altamente improvável que tantos caminhos sensíveis estejam realmente expostos - indica fallback global ou SPA.";
            return result;
        }
        
        return result;
    }
    
    /// <summary>
    /// HEURÍSTICA 1: Captura o conteúdo base da homepage para comparação
    /// Permite detectar quando um caminho sensível retorna o mesmo conteúdo (falso-positivo)
    /// </summary>
    private async Task CaptureHomepageBaselineAsync(string target, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_readTimeout);
            
            var response = await _httpClient.GetAsync(target, HttpCompletionOption.ResponseContentRead, cts.Token);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                _homepageContent = NormalizeContent(content);
                _homepageContentLength = _homepageContent.Length;
            }
        }
        catch
        {
            // Se falhar ao capturar homepage, continua sem baseline
            _homepageContent = null;
            _homepageContentLength = 0;
        }
    }

    /// <summary>
    /// Método que apenas coleta dados brutos (sem aplicar heurísticas ainda)
    /// Usado para detectar fallback global antes de filtrar falsos-positivos
    /// </summary>
    private async Task<ProbeResult?> ProbePathRawAsync(string target, string path, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_connectTimeout);

            string requestUri = $"{target}{path}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            var statusCode = response.StatusCode;
            
            // Só processa respostas HTTP 200 para análise de fallback
            if (statusCode == HttpStatusCode.OK)
            {
                using var contentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                contentCts.CancelAfter(_readTimeout);
                
                var content = await ReadLimitedContentAsync(response, contentCts.Token);
                var normalizedContent = NormalizeContent(content);
                
                return new ProbeResult
                {
                    Path = path,
                    StatusCode = statusCode,
                    Content = normalizedContent,
                    ContentLength = normalizedContent.Length,
                    IsPositiveFinding = false // Ainda não processado
                };
            }
            
            // Para outros status codes (401, 403, redirects), também rastreia
            return new ProbeResult
            {
                Path = path,
                StatusCode = statusCode,
                Content = string.Empty,
                ContentLength = 0,
                IsPositiveFinding = false
            };
        }
        catch
        {
            return null; // Timeout ou erro de rede
        }
    }
    
    /// <summary>
    /// Aplica heurísticas em um ProbeResult para determinar se é achado válido
    /// Chamado após a detecção de fallback global
    /// </summary>
    private async Task<JObject?> ApplyHeuristicsAsync(ProbeResult probeData, string target, CancellationToken cancellationToken)
    {
        var result = new JObject
        {
            ["path"] = probeData.Path,
            ["exists"] = false,
            ["statusCode"] = (int)probeData.StatusCode,
            ["contentLength"] = probeData.ContentLength,
            ["redirectLocation"] = null,
            ["evidence"] = null,
            ["severity"] = "Baixo",
            ["falsePositiveReason"] = null,
            ["_rawContent"] = probeData.Content,
            ["_validFinding"] = false
        };

        var statusCode = (int)probeData.StatusCode;
        
        // Ignora códigos inválidos
        if (statusCode == 404 || statusCode >= 500)
        {
            return null;
        }
        
        // Códigos que indicam recursos existentes
        if (statusCode == 401)
        {
            result["exists"] = true;
            result["severity"] = "Medio";
            result["evidence"] = "Requer autenticação (401)";
            result["_validFinding"] = true;
            return result;
        }

        if (statusCode == 403)
        {
            result["exists"] = true;
            result["severity"] = "Baixo";
            result["evidence"] = "Acesso negado (403)";
            result["_validFinding"] = true;
            return result;
        }

        // Redirects - faz nova requisição para obter location
        if (statusCode >= 300 && statusCode < 400)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(_connectTimeout);
                
                string requestUri = $"{target}{probeData.Path}";
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                
                var location = response.Headers.Location?.ToString();
                result["redirectLocation"] = location;
                
                if (IsLoginRedirect(location))
                {
                    result["falsePositiveReason"] = "Redirect global para página de login";
                    return null;
                }
                
                if (probeData.Path.Contains("/admin") || probeData.Path.Contains("/manager"))
                {
                    result["exists"] = true;
                    result["severity"] = "Baixo";
                    result["evidence"] = $"Redirect para: {location}";
                    result["_validFinding"] = true;
                    return result;
                }
            }
            catch
            {
                // Se falhar ao obter redirect, ignora
            }
            
            return null;
        }

        // HTTP 200 - aplica heurísticas
        if (statusCode >= 200 && statusCode < 300)
        {
            if (string.IsNullOrWhiteSpace(probeData.Content))
            {
                result["falsePositiveReason"] = "Resposta sem conteúdo";
                return null;
            }

            // Heurística 3: Detectar página de erro
            // Reconstrói conteúdo HTML para análise (desnormalizar temporariamente)
            if (LooksLikeErrorPageNormalized(probeData.Content))
            {
                result["falsePositiveReason"] = "Página de erro personalizada detectada";
                return null;
            }

            // Heurística 4: Comparar com homepage
            if (IsSameAsHomepage(probeData.Content))
            {
                result["falsePositiveReason"] = "Conteúdo idêntico à homepage";
                return null;
            }

            // Passou pelas heurísticas
            result["exists"] = true;
            result["severity"] = DetermineSeverity(probeData.Path, probeData.StatusCode);
            result["_validFinding"] = true;
            
            // Para extrair evidência, usa conteúdo normalizado (já está em lowercase)
            var evidence = ExtractEvidenceNormalized(probeData.Path, probeData.Content);
            if (!string.IsNullOrEmpty(evidence))
            {
                result["evidence"] = evidence;
            }

            return result;
        }

        return null;
    }
    
    /// <summary>
    /// Método wrapper que retorna tanto o resultado quanto os dados brutos para análise
    /// Agora SEMPRE retorna ProbeResult para análise de fallback global
    /// </summary>
    private async Task<(JObject result, ProbeResult? probeData)> ProbePathWithDataAsync(string target, string path, CancellationToken cancellationToken)
    {
        var result = await ProbePathAsync(target, path, cancellationToken);
        
        // SEMPRE cria ProbeResult para análise de fallback, incluindo falsos-positivos
        ProbeResult? probeData = null;
        
        // Só cria ProbeResult se tiver dados válidos (statusCode != 0)
        var statusCode = result["statusCode"]?.Value<int>() ?? 0;
        if (statusCode > 0)
        {
            probeData = new ProbeResult
            {
                Path = path,
                StatusCode = (HttpStatusCode)statusCode,
                Content = result["_rawContent"]?.Value<string>() ?? string.Empty,
                ContentLength = result["contentLength"]?.Value<int>() ?? 0,
                IsPositiveFinding = result["_validFinding"]?.Value<bool>() ?? false
            };
        }
        
        return (result, probeData);
    }
    
    /// <summary>
    /// 🔍 Método principal de sondagem com heurísticas anti-falso-positivo
    /// SEMPRE retorna JObject (nunca null) para permitir análise de fallback global
    /// </summary>
    private async Task<JObject> ProbePathAsync(string target, string path, CancellationToken cancellationToken)
    {
        var result = new JObject
        {
            ["path"] = path,
            ["exists"] = false,
            ["statusCode"] = 0,
            ["contentLength"] = 0,
            ["redirectLocation"] = null,
            ["evidence"] = null,
            ["severity"] = "Baixo",
            ["falsePositiveReason"] = null,
            ["_rawContent"] = string.Empty, // Campo interno para análise de fallback
            ["_validFinding"] = false // Indica se é um achado válido para incluir no resultado final
        };

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_connectTimeout);

            string requestUri = $"{target}{path}";

            // 📌 Usa GET diretamente (mais confiável que HEAD para detecção)
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            result["statusCode"] = (int)response.StatusCode;
            var statusCode = (int)response.StatusCode;

            // 🚫 REGRA 1: Bloquear códigos inválidos (4xx, 5xx, 3xx)
            if (statusCode >= 300 && statusCode < 400)
            {
                // Redirect detectado
                var location = response.Headers.Location?.ToString();
                result["redirectLocation"] = location;
                
                // 🔍 HEURÍSTICA 2: Detectar redirect para login (falso-positivo)
                if (IsLoginRedirect(location))
                {
                    result["falsePositiveReason"] = "Redirect global para página de login";
                    return result; // Retorna com _validFinding = false
                }
                
                // Redirect 301/302 pode ser legítimo em alguns casos específicos
                if (path.Contains("/admin") || path.Contains("/manager"))
                {
                    result["exists"] = true;
                    result["severity"] = "Baixo";
                    result["evidence"] = $"Redirect para: {location}";
                    result["_validFinding"] = true;
                    return result;
                }
                
                return result; // Outros redirects, _validFinding = false
            }

            if (statusCode == 404 || statusCode >= 500)
            {
                return result; // Não existe, _validFinding = false
            }

            // ✅ REGRA 2: Códigos válidos (200, 204, 401, 403)
            if (statusCode == 401)
            {
                // Requer autenticação - recurso existe e está protegido
                result["exists"] = true;
                result["severity"] = "Medio";
                result["evidence"] = "Requer autenticação (401)";
                result["_validFinding"] = true;
                return result;
            }

            if (statusCode == 403)
            {
                // Recurso existe mas acesso negado
                result["exists"] = true;
                result["severity"] = "Baixo";
                result["evidence"] = "Acesso negado (403)";
                result["_validFinding"] = true;
                return result;
            }

            if (statusCode >= 200 && statusCode < 300)
            {
                // Ler conteúdo para heurísticas avançadas
                using var contentCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                contentCts.CancelAfter(_readTimeout);
                
                var content = await ReadLimitedContentAsync(response, contentCts.Token);
                var normalizedContent = NormalizeContent(content);
                
                result["_rawContent"] = normalizedContent;
                result["contentLength"] = normalizedContent.Length;
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    // Resposta vazia - possível falso-positivo
                    result["falsePositiveReason"] = "Resposta sem conteúdo";
                    return result; // _validFinding = false
                }

                // HEURÍSTICA 3: Detectar página de erro personalizada
                if (LooksLikeErrorPage(content))
                {
                    result["falsePositiveReason"] = "Página de erro personalizada detectada";
                    return result; // _validFinding = false
                }

                // 🔍 HEURÍSTICA 4: Comparar com homepage (mesmo conteúdo = falso-positivo)
                if (IsSameAsHomepage(content))
                {
                    result["falsePositiveReason"] = "Conteúdo idêntico à homepage";
                    return result; // _validFinding = false
                }

                // Passou por todas as heurísticas - é um achado real
                result["exists"] = true;
                result["severity"] = DetermineSeverity(path, response.StatusCode);
                result["_validFinding"] = true;
                
                // Extrai evidência específica do caminho
                var evidence = ExtractEvidence(path, content);
                if (!string.IsNullOrEmpty(evidence))
                {
                    result["evidence"] = evidence;
                }

                return result;
            }

            return result; // _validFinding = false
        }
        catch (TaskCanceledException)
        {
            // Timeout - ignora silenciosamente
            return result; // _validFinding = false
        }
        catch (HttpRequestException)
        {
            // Erro de rede - ignora
            return result; // _validFinding = false
        }
        catch (Exception ex)
        {
            result["exception"] = ex.Message;
            return result; // _validFinding = false
        }
    }
    
    /// <summary>
    /// 🔍 HEURÍSTICA 2: Detecta se é um redirect para página de login (falso-positivo comum)
    /// </summary>
    private static bool IsLoginRedirect(string? location)
    {
        if (string.IsNullOrEmpty(location))
            return false;

        var locationLower = location.ToLowerInvariant();
        
        var loginPatterns = new[]
        {
            "/login", "/auth", "/signin", "/account/login",
            "/user/login", "/admin/login", "/sso", "/oauth"
        };

        return loginPatterns.Any(pattern => locationLower.Contains(pattern));
    }

    /// <summary>
    /// HEURÍSTICA 3: Detecta padrões comuns de páginas de erro personalizadas
    /// Versão para conteúdo normalizado
    /// </summary>
    private static bool LooksLikeErrorPageNormalized(string normalizedContent)
    {
        if (string.IsNullOrEmpty(normalizedContent))
            return false;
            
        // Padrões típicos de páginas de erro (já em lowercase por causa da normalização)
        var errorPatterns = new[]
        {
            "404", "not found", "página não encontrada", "page not found",
            "not available", "does not exist", "não existe", "erro 404",
            "error 404", "file not found", "the requested url",
            "oops", "something went wrong", "algo deu errado",
            "no encontrada", "página inexistente"
        };

        int errorMatches = errorPatterns.Count(pattern => normalizedContent.Contains(pattern));
        
        // Se encontrar 2+ padrões de erro, provavelmente é página de erro
        return errorMatches >= 2;
    }

    /// <summary>
    /// HEURÍSTICA 3: Detecta padrões comuns de páginas de erro personalizadas
    /// </summary>
    private static bool LooksLikeErrorPage(string content)
    {
        var contentLower = content.ToLowerInvariant();
        
        // Padrões típicos de páginas de erro
        var errorPatterns = new[]
        {
            "404", "not found", "página não encontrada", "page not found",
            "not available", "does not exist", "não existe", "erro 404",
            "error 404", "file not found", "the requested url",
            "oops", "something went wrong", "algo deu errado",
            "no encontrada", "página inexistente"
        };

        int errorMatches = errorPatterns.Count(pattern => contentLower.Contains(pattern));
        
        // Se encontrar 2+ padrões de erro, provavelmente é página de erro
        if (errorMatches >= 2)
            return true;

        // Títulos HTML típicos de erro
        var titleMatch = Regex.Match(content, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase);
        if (titleMatch.Success)
        {
            var title = titleMatch.Groups[1].Value.ToLowerInvariant();
            if (errorPatterns.Any(pattern => title.Contains(pattern)))
                return true;
        }

        // Detecta mensagens genéricas de erro no h1/h2
        var headingMatch = Regex.Match(content, @"<h[12][^>]*>(.*?)</h[12]>", RegexOptions.IgnoreCase);
        if (headingMatch.Success)
        {
            var heading = headingMatch.Groups[1].Value.ToLowerInvariant();
            if (errorPatterns.Any(pattern => heading.Contains(pattern)))
                return true;
        }

        return false;
    }

    /// <summary>
    /// HEURÍSTICA 4: Compara conteúdo com a homepage
    /// Se for muito similar (>85% match), é provável falso-positivo
    /// </summary>
    private bool IsSameAsHomepage(string content)
    {
        if (string.IsNullOrEmpty(_homepageContent) || string.IsNullOrEmpty(content))
            return false;

        var normalizedContent = NormalizeContent(content);
        
        // Comparação rápida de tamanho
        if (Math.Abs(normalizedContent.Length - _homepageContentLength) < 100)
        {
            // Tamanhos similares - fazer comparação de similaridade
            var similarity = CalculateSimilarity(_homepageContent, normalizedContent);
            return similarity > 0.85; // 85% de similaridade = falso-positivo
        }

        return false;
    }

    /// <summary>
    /// Normaliza conteúdo HTML para comparação (remove espaços, quebras, etc)
    /// </summary>
    private static string NormalizeContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        // Remove scripts e estilos
        content = Regex.Replace(content, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        content = Regex.Replace(content, @"<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        // Remove tags HTML
        content = Regex.Replace(content, @"<[^>]+>", " ");
        
        // Normaliza espaços
        content = Regex.Replace(content, @"\s+", " ");
        
        return content.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Calcula similaridade simples entre dois textos (baseado em Jaccard)
    /// </summary>
    private static double CalculateSimilarity(string text1, string text2)
    {
        var words1 = new HashSet<string>(text1.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var words2 = new HashSet<string>(text2.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (words1.Count == 0 || words2.Count == 0)
            return 0;

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersection / union;
    }

    /// <summary>
    /// Lê conteúdo com limite de tamanho (primeiros 4KB) para análise
    /// </summary>
    private static async Task<string> ReadLimitedContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        const int maxBytes = 4096; // 4KB suficiente para heurísticas
        
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        
        var buffer = new char[maxBytes];
        var bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);
        
        return new string(buffer, 0, bytesRead);
    }

    /// <summary>
    /// Extrai evidência específica baseada no caminho e conteúdo normalizado
    /// </summary>
    private static string? ExtractEvidenceNormalized(string path, string normalizedContent)
    {
        if (string.IsNullOrEmpty(normalizedContent))
            return null;

        // Evidências específicas por tipo de caminho (conteúdo já está em lowercase)
        if (path.Contains(".env"))
        {
            if (normalizedContent.Contains("db_password") || normalizedContent.Contains("database") || 
                normalizedContent.Contains("app_key") || normalizedContent.Contains("api_key"))
                return "Arquivo .env exposto com credenciais";
        }

        if (path.Contains(".git"))
        {
            if (normalizedContent.Contains("ref: refs/heads") || Regex.IsMatch(normalizedContent, @"commit [0-9a-f]{7,}"))
                return "Repositório .git exposto";
        }

        if (path.Contains("phpinfo"))
        {
            if (normalizedContent.Contains("phpinfo()") || normalizedContent.Contains("php version"))
                return "phpinfo() exposto - informações sensíveis do servidor";
        }

        if (path.Contains("phpmyadmin") || path.Contains("/pma"))
        {
            if (normalizedContent.Contains("phpmyadmin") || normalizedContent.Contains("database"))
                return "Interface phpMyAdmin acessível";
        }

        if (path.EndsWith(".sql") || path.Contains("backup"))
        {
            if (normalizedContent.Contains("insert into") || normalizedContent.Contains("create table") || 
                normalizedContent.Contains("mysqldump"))
                return "Backup SQL exposto";
        }

        if (path.Contains("web.config"))
        {
            if (normalizedContent.Contains("configuration") || normalizedContent.Contains("connectionstring"))
                return "Arquivo web.config exposto";
        }

        if (path.Contains("server-status"))
        {
            if (normalizedContent.Contains("server uptime") || normalizedContent.Contains("apache"))
                return "Apache server-status exposto";
        }

        if (path.Contains("actuator"))
        {
            if (normalizedContent.Contains("status") || normalizedContent.Contains("health"))
                return "Spring Boot Actuator exposto";
        }

        if (path.Contains("wp-login") || path.Contains("wp-admin"))
        {
            if (normalizedContent.Contains("wordpress") || normalizedContent.Contains("wp-login"))
                return "WordPress admin/login detectado";
        }

        if (path.Contains("/admin"))
        {
            return "Painel administrativo acessível";
        }

        return "Recurso sensível acessível";
    }

    /// <summary>
    /// Extrai evidência específica baseada no caminho e conteúdo
    /// </summary>
    private static string? ExtractEvidence(string path, string content)
    {
        var contentLower = content.ToLowerInvariant();

        // Evidências específicas por tipo de caminho
        if (path.Contains(".env"))
        {
            if (contentLower.Contains("db_password") || contentLower.Contains("database") || 
                contentLower.Contains("app_key") || contentLower.Contains("api_key"))
                return "Arquivo .env exposto com credenciais";
        }

        if (path.Contains(".git"))
        {
            if (contentLower.Contains("ref: refs/heads") || Regex.IsMatch(contentLower, @"commit [0-9a-f]{7,}"))
                return "Repositório .git exposto";
        }

        if (path.Contains("phpinfo"))
        {
            if (contentLower.Contains("phpinfo()") || contentLower.Contains("php version"))
                return "phpinfo() exposto - informações sensíveis do servidor";
        }

        if (path.Contains("phpmyadmin") || path.Contains("/pma"))
        {
            if (contentLower.Contains("phpmyadmin") || contentLower.Contains("database"))
                return "Interface phpMyAdmin acessível";
        }

        if (path.EndsWith(".sql") || path.Contains("backup"))
        {
            if (contentLower.Contains("insert into") || contentLower.Contains("create table") || 
                contentLower.Contains("mysqldump"))
                return "Backup SQL exposto";
        }

        if (path.Contains("web.config"))
        {
            if (contentLower.Contains("<configuration>") || contentLower.Contains("connectionstring"))
                return "Arquivo web.config exposto";
        }

        if (path.Contains("server-status"))
        {
            if (contentLower.Contains("server uptime") || contentLower.Contains("apache"))
                return "Apache server-status exposto";
        }

        if (path.Contains("actuator"))
        {
            if (contentLower.Contains("\"status\":") || contentLower.Contains("health"))
                return "Spring Boot Actuator exposto";
        }

        if (path.Contains("wp-login") || path.Contains("wp-admin"))
        {
            if (contentLower.Contains("wordpress") || contentLower.Contains("wp-login"))
                return "WordPress admin/login detectado";
        }

        if (path.Contains("/admin"))
        {
            // Extrai título se houver
            var titleMatch = Regex.Match(content, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase);
            if (titleMatch.Success)
            {
                var title = titleMatch.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(title) && title.Length < 100)
                    return $"Painel administrativo: {title}";
            }
            return "Painel administrativo acessível";
        }

        // Genérico: tenta extrair título da página
        var genericTitle = Regex.Match(content, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase);
        if (genericTitle.Success)
        {
            var title = genericTitle.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(title) && title.Length < 150)
                return title;
        }

        return "Recurso sensível acessível";
    }

    /// <summary>
    ///  Determinar a severidade com base no código HTTP e no path
    /// </summary>
    private static string DetermineSeverity(string path, HttpStatusCode code)
    {
        int sc = (int)code;

        // Códigos que indicam recursos protegidos mas existentes
        if (sc == 401) return "Medio";
        if (sc == 403) return "Baixo";

        // Para 200, analisa o path
        if (sc >= 200 && sc < 300)
        {
            // Caminhos críticos (vazamento de dados)
            if (path.Contains("/backup") || path.EndsWith(".sql") || path.Contains(".env") || 
                path.Contains(".git") || path.Contains(".bash_history") || path.Contains("/.ssh"))
                return "Critico";
            
            // Caminhos de alto risco (painéis administrativos, info leak)
            if (path.Contains("phpinfo") || path.Contains("phpmyadmin") || path.Contains("adminer") ||
                path.Contains("/manager/html") || path.Contains("/actuator/env"))
                return "Alto";
            
            // Admin panels
            if (path.Contains("/admin") || path.Contains("wp-login") || path.Contains("/manager") ||
                path.Contains("/console") || path.Contains("administrator"))
                return "Alto";
            
            // Médio risco (endpoints de debug, status)
            if (path.Contains("/actuator") || path.Contains("/server-status") || path.Contains("/solr") ||
                path.Contains("/.well-known"))
                return "Medio";
            
            return "Medio";
        }

        return "Baixo";
    }
}
