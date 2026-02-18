using Microsoft.Extensions.Configuration;
using System.Text;

namespace HeimdallWeb.Application.Services.AI;

/// <summary>
/// Service for analyzing security scan results using Google Gemini AI.
/// Refactored from HeimdallWebOld/Services/IA/GeminiService.cs
/// </summary>
public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/";
    private readonly HttpClient _httpClient;

    public GeminiService(IConfiguration config)
    {
        _apiKey = config["GEMINI_API_KEY"] ?? throw new ArgumentNullException(nameof(config), "Gemini API Key not configured");
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_apiUrl)
        };
    }

    public async Task<string> AnalyzeScanResultsAsync(string scanJson, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            contents = new[]
            {
                new {
                    parts = new[]
                    {
                        new
                        {
                            text = $@"
                                Você é um assistente de cibersegurança integrado ao sistema HeimdallWeb.
                                Seu objetivo é analisar resultados de varreduras de sites e aplicações, identificando riscos e vulnerabilidades.

                                ### Scanners Disponíveis (13 scanners em paralelo):
                                O JSON de entrada contém os resultados combinados de TODOS os scanners executados:
                                1.  **HeaderScanner** (chaves: `headers`, `securityHeaders`, `cookies`) — Headers HTTP e cookies.
                                2.  **SslScanner** (`resultsSslScanner`) — Certificado SSL: sujeito, emissor, validade, key size.
                                3.  **PortScanner** (`resultsPortScanner`) — Portas TCP abertas com banners.
                                4.  **SensitivePathsScanner** (`sensitivePathScanner`) — Caminhos sensíveis expostos.
                                5.  **HttpRedirectScanner** (`resultsHttpRedirectScanner`) — Redirecionamento HTTP→HTTPS.
                                6.  **RobotsScanner** (`robots`) — Robots.txt e Sitemap.
                                7.  **TlsCapabilityScanner** (`tls_capability`) — Suporte TLS 1.2/1.3, cipher suite negociado, detecção de ciphers fracos via `SslStream`.
                                8.  **CspAnalyzerScanner** (`csp_analysis`) — Análise semântica do header `Content-Security-Policy`: detecta `unsafe-inline`, `unsafe-eval`, wildcards `*`, diretivas ausentes.
                                9.  **DomainAgeScanner** (`domain_age`) — WHOIS lookup: data de criação, idade em dias. Domínios com <90 dias são suspeitos.
                                10. **IpChangeScanner** (`ip_resolution`) — Resolução DNS (IPv4/IPv6), detecção de CDN (Cloudflare, Fastly, Akamai).
                                11. **ResponseBehaviorScanner** (`response_behavior`) — Time To First Byte (TTFB em ms), verificação de 404 correto (soft 404 detection).
                                12. **SubdomainDiscoveryScanner** (`subdomains`) — Descoberta de subdomínios comuns via DNS (www, api, dev, staging, admin, mail, ftp, vpn, portal).
                                13. **SecurityTxtScanner** (`security_txt`) — Presença e validade de `/.well-known/security.txt` (RFC 9116): contato, criptografia, expiração.

                                ### Objetivos:
                                - Interpretar saídas de TODOS os 13 scanners, correlacionando dados entre eles.
                                - Usar banners de portas e headers para confirmar tecnologia, versão e possíveis falhas conhecidas.
                                - Identificar vulnerabilidades como XSS, SQL Injection, Headers e Cookies Inseguros, SSL inválido, TLS fraco, CSP permissivo, domínio jovem (phishing), TTFB alto, exposição de subdomínios, falta de security.txt, Robots.txt e Sitemap, exposição de portas críticas.
                                - Classificar cada achado em categorias: **SSL/TLS, CSP Analysis, Headers de Segurança, Cookies Inseguros, Portas Abertas, Redirecionamento, Domain Reputation, Infra Change, Response Behavior, Compliance, Injeção, XSS, CSRF, Autenticação, Autorização, Exposição de Dados, Configuração, Path Traversal, File Upload, CORS, API Security, DoS/DDoS, Criptografia, Session Management, Input Validation, Information Disclosure, Outros**.
                                - Use a categoria **Outros** APENAS quando a vulnerabilidade não se encaixar em NENHUMA das categorias acima.
                                - Retornar respostas **curtas, objetivas e padronizadas**, para economizar tokens.
                                - Sempre classificar o risco em: **Informativo | Baixo | Medio | Alto | Critico** (igual ao ENUM do sistema).
                                - Use **Informativo** para observações relevantes sem risco direto (fallback global detectado, headers recomendados ausentes mas não críticos, SSL válido próximo da expiração com >30 dias, configurações de SPA/catch-all, portas abertas não-críticas, CDN detectado corretamente, security.txt presente, etc.).
                                - **ATENÇÃO ESPECIAL para novos scanners**:
                                  * `tls_capability`: Se `weak_cipher_detected=true`, classificar como **Alto** em SSL/TLS. Se apenas TLS 1.2 sem TLS 1.3, classificar como **Baixo**.
                                  * `csp_analysis`: `unsafe-inline` ou `unsafe-eval` → **Medio** em CSP Analysis. CSP ausente (`csp_present=false`) → **Alto**.
                                  * `domain_age`: Domínio com <90 dias → **Medio** em Domain Reputation (possível phishing/scam). <30 dias → **Alto**.
                                  * `ip_resolution`: CDN detectado é **Informativo** positivo. Múltiplos IPs sem CDN pode indicar load balancing (Informativo).
                                  * `response_behavior`: TTFB >2000ms → **Medio** em Response Behavior. `returns_proper_404=false` → **Baixo** em Configuração (soft 404/SEO issue).
                                  * `subdomains`: Subdomínios como `admin`, `staging`, `dev` expostos → **Medio** em Exposição de Dados. Subdomínios normais (www, mail) → **Informativo**.
                                  * `security_txt`: Ausência (`present=false`) → **Baixo** em Compliance. Presente e válido → **Informativo** positivo.
                                - Se o scanner de caminhos sensíveis retornar `""status"": ""suspected-fallback""` ou `""type"": ""global-fallback""`, classificar como **Informativo** (configuração comum em SPAs).
                                - Fornecer recomendações práticas de mitigação.

                                ### Estrutura de resposta (JSON válido):
                                {{
                                    ""alvo"": ""domínio ou IP analisado"",
                                    ""resumo"": ""Resumo em até 5 linhas"",
                                    ""achados"": [
                                    {{
                                        ""descricao"": ""Explicação breve do problema"",
                                        ""categoria"": ""SSL/TLS | CSP Analysis | Headers de Segurança | Cookies Inseguros | Portas Abertas | Redirecionamento | Domain Reputation | Infra Change | Response Behavior | Compliance | Injeção | XSS | CSRF | Autenticação | Autorização | Exposição de Dados | Configuração | Path Traversal | File Upload | CORS | API Security | DoS/DDoS | Criptografia | Session Management | Input Validation | Information Disclosure | Outros"",
                                        ""risco"": ""Informativo | Baixo | Medio | Alto | Critico"",
                                        ""evidencia"": ""Trecho do JSON analisado que comprova a vulnerabilidade"",
                                        ""recomendacao"": ""Sugestão de mitigação""
                                    }}
                                    ],
                                    ""tecnologias"": [
                                    {{
                                        ""nome_tecnologia"": ""Ex: Apache, Nginx, React, Express, Cloudflare, etc."",
                                        ""versao"": ""Versão detectada (se disponível)"",
                                        ""categoria_tecnologia"": ""Web Server | CMS | Framework | Database | Language | Frontend | API | OS | Security | CDN | Reverse Proxy | Email | Analytics | Cloud | Cache | DevOps"",
                                        ""descricao_tecnologia"": ""descrição **simples e objetiva** da tecnologia, com **no máximo 7 linhas**, incluindo seu uso principal e eventuais **fraquezas ou riscos comuns** (ex: vulnerabilidades conhecidas em versões antigas, má configuração, exposição de painéis administrativos, etc, se a versão for detectada tente se aprofundar nela.)""
                                    }}
                                    ]
                                }}

                                ### Regras:
                                1. Nunca retornar texto fora do JSON.
                                2. Se não houver vulnerabilidades críticas, ainda listar observações relevantes (headers ausentes, SSL perto da expiração, CSP permissivo, domínio jovem, TTFB alto, subdomínios expostos, security.txt ausente, etc.).
                                3. Resumir sempre que possível, sem repetir informações.
                                4. Se a entrada não for um log de scan, retornar:
                                    {{ ""alvo"": """", ""resumo"": ""Entrada inválida"", ""achados"": [], ""tecnologias"": [] }}
                                5. Não incluir no JSON itens totalmente irrelevantes. Itens seguros mas informativos devem usar nível **Informativo** (ex: SSL válido mas perto da expiração, headers recomendados ausentes, fallback global detectado, CDN ativo, security.txt válido).
                                6. Se possível, mapear as tecnologias detectadas (a partir de banners, headers, certificados, CDN provider, ou respostas HTTP)
                                    dentro da chave ""tecnologias"", preenchendo ""nome_tecnologia"" e ""versao"".
                                    Se `ip_resolution.cdn_provider` indicar uma CDN (ex: Cloudflare), incluí-la como tecnologia na categoria ""CDN"".
                                    Caso nenhuma tecnologia seja identificada, retornar um array vazio.
                                7. Se o campo esperado não tiver valor disponível, retornar valor nulo no json (null), pois o valor deve ser tratado como nulo,
                                    **nunca** as strings """"null"""", """"json null"""", """"undefined"""" ou qualquer texto fora do formato JSON.
                                8. Não forneça de forma alguma conteúdo repetido.
                                9. Correlacione dados entre scanners: ex, se `tls_capability` mostra cipher fraco E `resultsSslScanner` mostra certificado válido, gere um achado combinado em vez de dois separados.
                                10. **IMPORTANTE**: GARANTIR que todo o CONTEÚDO (valores) do JSON esteja em PORTUGUÊS (PT-BR). As CHAVES do JSON devem permanecer EXATAMENTE como no exemplo acima (em português: ""alvo"", ""resumo"", ""achados"", etc.). NÃO altere a estrutura do JSON.

                                ### JSON de entrada:
                                {scanJson}
                            "
                        }
                    }
                }
            }
        };

        try
        {
            // Create JSON content
            var content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            // Send POST request to Gemini API
            var response = await _httpClient.PostAsync(
                $"models/gemini-2.5-flash:generateContent?key={_apiKey}",
                content,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync(cancellationToken);

            // Parse response
            var parsedResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);
            var aiResponseText = parsedResult?.candidates?[0]?.content?.parts?[0]?.text?.ToString()
                ?? "No response generated by AI.";

            // Remove markdown formatting if present
            aiResponseText = RemoveMarkdown(aiResponseText);

            // Correct wrong null values (e.g., string "null" -> actual null)
            aiResponseText = CorrectWrongNullValues(aiResponseText);

            return aiResponseText;
        }
        catch (Exception ex)
        {
            return $"{{\"alvo\": \"\", \"resumo\": \"Error communicating with Gemini AI: {ex.Message}\", \"achados\": [], \"tecnologias\": []}}";
        }
    }

    /// <summary>
    /// Removes markdown code block markers from AI response.
    /// </summary>
    private static string RemoveMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Remove ```json and ``` markers
        text = text.Replace("```json", "").Replace("```", "").Trim();
        return text;
    }

    /// <summary>
    /// Corrects string representations of null to actual null values in JSON.
    /// </summary>
    private static string CorrectWrongNullValues(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText))
            return jsonText;

        // Replace string "null" with actual null
        jsonText = jsonText.Replace("\"null\"", "null");
        jsonText = jsonText.Replace("\"json null\"", "null");
        jsonText = jsonText.Replace("\"undefined\"", "null");

        return jsonText;
    }
}
