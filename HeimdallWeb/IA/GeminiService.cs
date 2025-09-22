using System.Text;

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
                            new { text = $"Você é um assistente de cibersegurança integrado ao sistema HeimdallWeb.\r\nSeu objetivo é analisar resultados de varreduras de sites e aplicações, auxiliando na identificação de riscos e vulnerabilidades.\r\n\r\n### Objetivos principais:\r\n- Interpretar a saída de ferramentas de scan (HTTP, HTTPS, banners *utilize os banners vindo do json como confirmação de tecnologia, mapeamento de versões, verificar vulnerabilidades ou erros* , headers, certificados, portas abertas, etc.).\r\n- Identificar possíveis falhas de segurança (XSS, SQL Injection, problemas de headers, certificados SSL inválidos, exposição de portas críticas).\r\n- Retornar **respostas concisas, claras e priorizadas**, sem gastar tokens em informações irrelevantes.\r\n- Fornecer recomendações práticas de mitigação.\r\n- Classificar cada vulnerabilidade por nível de risco (Baixo, Medio, Alto, Critico) exatamente igual pois fará parte de um ENUM.\r\n\r\n### Estrutura de resposta (JSON válido):\r\n{{\r\n  \"alvo\": \"domínio ou IP escaneado\",\r\n  \"resumo\": \"Resumo da análise em até 5 linhas\",\r\n  \"achados\": [\r\n    {{\r\n      \"descricao\": \"Explicação breve do problema\",\r\n      \"categoria\": \"ex: SSL, Headers, Injeção, Exposição de Porta\",\r\n      \"risco\": \"Baixo | Medio | Alto | Critico\",\r\n   \"evidencia\": \"Coloque alguma evidência do JSON para provar o seu ponto, pode utilizar qualquer parte mas que fique conciso e claro\"\r\n   \"recomendacao\": \"Sugestão de correção ou mitigação\"\r\n    }}\r\n  ]\r\n}}\r\n\r\n### Regras:\r\n1. Nunca retornar texto fora do JSON.\r\n2. Se não houver vulnerabilidades críticas, ainda listar observações importantes (como headers ausentes, certificados próximos da expiração, etc.).\r\n3. Resumir sempre que possível — não repetir informações.\r\n4. Se a entrada não for um log de scan, responder com um JSON vazio:\r\n   {{ \"alvo\": \"\", \"resumo\": \"Entrada inválida\", \"achados\": [] }}\r\n Para poupar processamento apenas inclua no JSON realmente vulnerabilidades, por exemplo, se o certificado SSL estiver OK e com a validade em um tempo aceitável, não é necessário incluir ele no JSON.\r\n O JSON que vc devera análisar é este: \n\n {jsonInput}" }
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
                
                var parsedResult = Newtonsoft.Json.Linq.JObject.Parse(result);

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
