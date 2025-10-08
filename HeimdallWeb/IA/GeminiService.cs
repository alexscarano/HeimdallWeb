using System.Text;
using HeimdallWeb.Helpers;

namespace HeimdallWeb.IA
{
    public class GeminiService
    {
        private readonly string _apiKey;

        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/";

        private readonly HttpClient _httpClient;

        public GeminiService(IConfiguration config)
        {
            _apiKey = config["GEMINI_API_KEY"] ?? throw new ArgumentNullException("Gemini API Key não configurada");
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_apiUrl)
            };
        }


        public async Task<string> GenerateTextAsyncFindings(string jsonInput)
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
                            new {
                                  text = $@"
                                        Você é um assistente de cibersegurança integrado ao sistema HeimdallWeb.
                                        Seu objetivo é analisar resultados de varreduras de sites e aplicações, identificando riscos e vulnerabilidades.

                                        ### Objetivos:
                                        - Interpretar saídas de scans (HTTP/HTTPS, banners, headers, certificados, portas, redirecionamentos).
                                        - Usar banners para confirmar tecnologia, versão e possíveis falhas conhecidas.
                                        - Identificar vulnerabilidades como XSS, SQL Injection, headers inseguros, SSL inválido, exposição de portas críticas.
                                        - Classificar cada achado em categorias fixas: **SSL, Headers, Portas, Redirecionamento, Injeção, Outros**.
                                        - Retornar respostas **curtas, objetivas e padronizadas**, para economizar tokens.
                                        - Sempre classificar o risco em: **Baixo | Medio | Alto | Critico** (igual ao ENUM do sistema).
                                        - Fornecer recomendações práticas de mitigação.

                                        ### Estrutura de resposta (JSON válido):
                                        {{
                                          ""alvo"": ""domínio ou IP analisado"",
                                          ""resumo"": ""Resumo em até 5 linhas"",
                                          ""achados"": [
                                            {{
                                              ""descricao"": ""Explicação breve do problema"",
                                              ""categoria"": ""SSL | Headers | Portas | Redirecionamento | Injeção | Outros"",
                                              ""risco"": ""Baixo | Medio | Alto | Critico"",
                                              ""evidencia"": ""Trecho do JSON analisado que comprova a vulnerabilidade"",
                                              ""recomendacao"": ""Sugestão de mitigação""
                                            }}
                                          ]
                                        }}

                                        ### Regras:
                                        1. Nunca retornar texto fora do JSON.
                                        2. Se não houver vulnerabilidades críticas, ainda listar observações relevantes (headers ausentes, SSL perto da expiração etc.).
                                        3. Resumir sempre que possível, sem repetir informações.
                                        4. Se a entrada não for um log de scan, retornar:
                                           {{ ""alvo"": """", ""resumo"": ""Entrada inválida"", ""achados"": [] }}
                                        5. Não incluir no JSON itens irrelevantes ou seguros (ex: SSL válido dentro da validade).

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
                
                var parsedResult = result.ToJson();

                return parsedResult["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString().RemoveMarkdown()
                    ?? "Nenhuma resposta gerada.";
            }
            catch (Exception err)
            {
                return $"Erro ao comunicar com o serviço Gemini. {err}";
            } 
        }
    }
}
