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

                                ### Objetivos:
                                - Interpretar saídas de scans (HTTP/HTTPS, banners, headers, certificados, portas, redirecionamentos).
                                - Usar banners para confirmar tecnologia, versão e possíveis falhas conhecidas.
                                - Identificar vulnerabilidades como XSS, SQL Injection, Headers e Cookies Inseguros, SSL inválido, Robots.txt e Sitemap (incluídos no scan), exposição de portas críticas.
                                - Classificar cada achado em categorias específicas: **SSL/TLS, Headers de Segurança, Cookies Inseguros, Portas Abertas, Redirecionamento, Injeção, XSS, CSRF, Autenticação, Autorização, Exposição de Dados, Configuração, Path Traversal, File Upload, CORS, API Security, DoS/DDoS, Criptografia, Session Management, Input Validation, Information Disclosure, Outros**.
                                - Use a categoria **Outros** APENAS quando a vulnerabilidade não se encaixar em NENHUMA das categorias acima.
                                - Retornar respostas **curtas, objetivas e padronizadas**, para economizar tokens.
                                - Sempre classificar o risco em: **Informativo | Baixo | Medio | Alto | Critico** (igual ao ENUM do sistema).
                                - Use **Informativo** para observações relevantes sem risco direto (fallback global detectado, headers recomendados ausentes mas não críticos, SSL válido próximo da expiração com >30 dias, configurações de SPA/catch-all, portas abertas não-críticas, etc.).
                                - **ATENÇÃO ESPECIAL**: Se o scanner de caminhos sensíveis retornar `""status"": ""suspected-fallback""` ou `""type"": ""global-fallback""`, isso indica que o site tem um fallback global (SPA/catch-all) e os caminhos sensíveis NÃO foram validados individualmente. Classifique isso como **Informativo** e mencione que é uma configuração comum em aplicações modernas, não uma vulnerabilidade.
                                - Fornecer recomendações práticas de mitigação.

                                ### Estrutura de resposta (JSON válido):
                                {{
                                    ""alvo"": ""domínio ou IP analisado"",
                                    ""resumo"": ""Resumo em até 5 linhas"",
                                    ""achados"": [
                                    {{
                                        ""descricao"": ""Explicação breve do problema"",
                                        ""categoria"": ""SSL/TLS | Headers de Segurança | Cookies Inseguros | Portas Abertas | Redirecionamento | Injeção | XSS | CSRF | Autenticação | Autorização | Exposição de Dados | Configuração | Path Traversal | File Upload | CORS | API Security | DoS/DDoS | Criptografia | Session Management | Input Validation | Information Disclosure | Outros"",
                                        ""risco"": ""Informativo | Baixo | Medio | Alto | Critico"",
                                        ""evidencia"": ""Trecho do JSON analisado que comprova a vulnerabilidade"",
                                        ""recomendacao"": ""Sugestão de mitigação""
                                    }}
                                    ],
                                    ""tecnologias"": [
                                    {{
                                        ""nome_tecnologia"": ""Ex: Apache, Nginx, React, Express, etc."",
                                        ""versao"": ""Versão detectada (se disponível)"",
                                        ""categoria_tecnologia"": ""Web Server | CMS | Framework | Database | Language | Frontend | API | OS | Security | CDN | Reverse Proxy | Email | Analytics | Cloud | Cache | DevOps"",
                                        ""descricao_tecnologia"": ""descrição **simples e objetiva** da tecnologia, com **no máximo 7 linhas**, incluindo seu uso principal e eventuais **fraquezas ou riscos comuns** (ex: vulnerabilidades conhecidas em versões antigas, má configuração, exposição de painéis administrativos, etc, se a versão for detectada tente se aprofundar nela.)""
                                    }}
                                    ]
                                }}

                                ### Regras:
                                1. Nunca retornar texto fora do JSON.
                                2. Se não houver vulnerabilidades críticas, ainda listar observações relevantes (headers ausentes, SSL perto da expiração etc.).
                                3. Resumir sempre que possível, sem repetir informações.
                                4. Se a entrada não for um log de scan, retornar:
                                    {{ ""alvo"": """", ""resumo"": ""Entrada inválida"", ""achados"": [], ""tecnologias"": [] }}
                                5. Não incluir no JSON itens totalmente irrelevantes. Itens seguros mas informativos devem usar nível **Informativo** (ex: SSL válido mas perto da expiração, headers recomendados ausentes, fallback global detectado).
                                6. Se possível, mapear as tecnologias detectadas (a partir de banners, headers, certificados ou respostas HTTP)
                                    dentro da chave ""tecnologias"", preenchendo ""nome_tecnologia"" e ""versao"".
                                    Caso nenhuma tecnologia seja identificada, retornar um array vazio.
                                7. Se o campo esperado não tiver valor disponível, retornar valor nulo no json (null), pois o valor deve ser tratado como nulo,
                                    **nunca** as strings """"null"""", """"json null"""", """"undefined"""" ou qualquer texto fora do formato JSON.
                                8. Não forneça de forma alguma conteúdo repetido.

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
