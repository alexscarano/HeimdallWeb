using ASHelpers.Extensions;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Enums;
using HeimdallWeb.Models;

namespace HeimdallWeb.Services.IA
{
    public class GeminiService : IGeminiService
    {
        private readonly string _apiKey;

        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/";

        private readonly HttpClient _httpClient;
        private readonly ILogRepository? _logRepository;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public GeminiService(IConfiguration config, ILogRepository? logRepository = null, IHttpContextAccessor? httpContextAccessor = null)
        {
            _apiKey = config["GEMINI_API_KEY"] ?? throw new ArgumentNullException("Gemini API Key não configurada");
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_apiUrl)
            };
            _logRepository = logRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GeneratePrompt(string jsonInput)
        {
            /*
             * Prepara o payload da requisição, exemplo de uso:
             curl "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent" \
              -H 'Content-Type: application/json' \
              -H 'X-goog-api-key: GEMINI_API_KEY' \
              -X POST \
              -d '{
                "contents": [
                  {
                    "parts": [
                      {
                        "text": "Explain how AI works in a few words"
                      }
                    ]
                  }
                ]
              }'
             */
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
                                    - Identificar vulnerabilidades como XSS, SQL Injection, Headers e Cookies Inseguros, SSL inválido, Robots.txt e Sitemap (incluidos no scan), exposição de portas críticas.
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
                                    {jsonInput}
                                "
                            }
                        }
                    }
                }
            };

            try
            {
                _logRepository?.AddLog(new LogModel
                {
                    code = LogEventCode.AI_REQUEST,
                    message = "Enviando requisição à IA",
                    source = "GeminiService",
                    remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor?.HttpContext)
                });

                // Cria o conteúdo da requisição em JSON
                var content = new StringContent(
                      Newtonsoft.Json.JsonConvert.SerializeObject(payload),
                      Encoding.UTF8,
                      "application/json"
                );

                // Envia a requisição POST para o endpoint do Gemini
                var response = await _httpClient.PostAsync(
                    $"models/gemini-2.0-flash:generateContent?key={_apiKey}",
                    content
                );

                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                var parsedResult = result.CorrectWrongNullValues().ToJson();

                var aiResponseText = parsedResult["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString().RemoveMarkdown()
                    ?? "Nenhuma resposta gerada.";

                _logRepository?.AddLog(new LogModel
                {
                    code = LogEventCode.AI_RESPONSE,
                    message = "Resposta da IA recebida com sucesso",
                    source = "GeminiService",
                    remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor?.HttpContext)
                });

                return aiResponseText;
            }
            catch (Exception err)
            {
                _logRepository?.AddLog(new LogModel
                {
                    code = LogEventCode.AI_RESPONSE_ERROR,
                    message = "Falha ao interpretar resposta da IA",
                    source = "GeminiService",
                    details = err.ToString(),
                    remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor?.HttpContext)
                });
                return $"Erro ao comunicar com o serviço Gemini. {err}";
            } 
        }
    }
}
