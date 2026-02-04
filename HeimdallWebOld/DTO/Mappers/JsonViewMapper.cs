using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.DTO.Mappers
{
    /// <summary>
    /// Mapper para converter JSON bruto do scanner em JsonViewPrettyDTO
    /// Extrai e organiza os dados técnicos para apresentação visual
    /// </summary>
    public static class JsonViewMapper
    {
        public static JsonViewPrettyDTO ToPrettyDTO(JObject rawScanJson, string target, string? summary)
        {
            // Dados gerais do scan
            var dadosGerais = ExtractSummary(rawScanJson);

            // Headers HTTP
            var headers = ExtractHeaders(rawScanJson);

            // Security Headers
            var securityHeaders = ExtractSecurityHeaders(rawScanJson);

            // Portas abertas
            var portas = ExtractPorts(rawScanJson);

            // Caminhos sensíveis
            var caminhos = ExtractSensitivePaths(rawScanJson);

            // Cookies
            var cookies = ExtractCookies(rawScanJson);

            // HTTP Redirects
            var httpRedirects = ExtractHttpRedirects(rawScanJson);

            // SSL/TLS
            var ssl = ExtractSslInfo(rawScanJson);

            // Robots.txt
            var robots = ExtractRobotsInfo(rawScanJson);

            // Formata JSON para exibição
            string formattedJson;
            try
            {
                var jsonString = rawScanJson.ToString();
                var jsonDoc = JsonDocument.Parse(jsonString);
                formattedJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            catch
            {
                formattedJson = rawScanJson.ToString();
            }

            return new JsonViewPrettyDTO(
                Alvo: target,
                Resumo: summary,
                DadosGerais: dadosGerais,
                Headers: headers,
                HeadersSeguranca: securityHeaders,
                Cookies: cookies,
                Portas: portas,
                CaminhosTestados: caminhos,
                HttpRedirects: httpRedirects,
                SSL: ssl,
                Robots: robots,
                JsonCompleto: formattedJson
            );
        }

        private static ScanResultSummaryDTO? ExtractSummary(JObject json)
        {
            try
            {
                var statusCode = json["statusCodeHttpRequest"]?.Value<int>();
                var servidor = json["headers"]?["Server"]?.ToString();
                
                // Verifica redirects HTTPS
                bool? redirectHTTPS = null;
                var resultsArray = json["resultsHttpRedirectScanner"] as JArray;
                if (resultsArray != null && resultsArray.Any())
                {
                    redirectHTTPS = resultsArray.Any(r => 
                        r["redirect_detected"]?.Value<bool>() == true && 
                        r["redirect_url"]?.ToString()?.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == true
                    );
                }

                var portsResults = json["resultsPortScanner"] as JArray;
                var portasAbertas = portsResults?.Count(p => p["open"]?.Value<bool>() == true) ?? 0;
                
                // Conta caminhos sensíveis encontrados
                int? caminhosSensiveis = null;
                var sensitivePathScanner = json["sensitivePathScanner"];
                if (sensitivePathScanner != null)
                {
                    var results = sensitivePathScanner["results"] as JArray;
                    caminhosSensiveis = results?.Count(r => r["exists"]?.Value<bool>() == true);
                }

                var totalCookies = json["cookies"]?.Count();

                return new ScanResultSummaryDTO(
                    StatusCode: statusCode,
                    Servidor: servidor,
                    RedirecionamentoHTTPS: redirectHTTPS,
                    TotalPortasAbertas: portasAbertas,
                    TotalCaminhosSensiveis: caminhosSensiveis,
                    TotalCookies: totalCookies
                );
            }
            catch
            {
                return null;
            }
        }

        private static List<HeaderInfoDTO>? ExtractHeaders(JObject json)
        {
            try
            {
                var headersObj = json["headers"] as JObject;
                if (headersObj == null) return null;

                var list = new List<HeaderInfoDTO>();
                foreach (var prop in headersObj.Properties())
                {
                    list.Add(new HeaderInfoDTO(
                        Nome: prop.Name,
                        Valor: prop.Value.ToString()
                    ));
                }
                return list.Any() ? list : null;
            }
            catch
            {
                return null;
            }
        }

        private static List<SecurityHeaderDTO>? ExtractSecurityHeaders(JObject json)
        {
            try
            {
                var secHeaders = json["securityHeaders"] as JObject;
                if (secHeaders == null) return null;

                var list = new List<SecurityHeaderDTO>();

                var present = secHeaders["present"] as JObject;
                if (present != null)
                {
                    foreach (var prop in present.Properties())
                    {
                        list.Add(new SecurityHeaderDTO(
                            Nome: prop.Name,
                            Status: "Presente",
                            Valor: prop.Value.ToString()
                        ));
                    }
                }

                var missing = secHeaders["missing"] as JArray;
                if (missing != null)
                {
                    foreach (var item in missing)
                    {
                        list.Add(new SecurityHeaderDTO(
                            Nome: item.ToString(),
                            Status: "Ausente",
                            Valor: null
                        ));
                    }
                }

                return list.Any() ? list : null;
            }
            catch
            {
                return null;
            }
        }

        private static List<PortInfoDTO>? ExtractPorts(JObject json)
        {
            try
            {
                var portsArray = json["resultsPortScanner"] as JArray;
                if (portsArray == null || !portsArray.Any()) return null;

                var list = new List<PortInfoDTO>();
                foreach (var probe in portsArray)
                {
                    var isOpen = probe["open"]?.Value<bool>() ?? false;
                    if (!isOpen) continue; // Só mostra portas abertas

                    var ip = probe["ip"]?.Value<string>() ?? "N/A";
                    var portNum = probe["port"]?.Value<int>() ?? 0;
                    var severity = probe["severity"]?.Value<string>() ?? "Informativo";
                    var description = probe["description"]?.Value<string>() ?? "Porta aberta";
                    var servico = GetServiceName(portNum);

                    list.Add(new PortInfoDTO(
                        IP: ip,
                        Porta: portNum,
                        Estado: "Aberta",
                        Servico: servico,
                        Severidade: severity,
                        Descricao: description
                    ));
                }
                return list.Any() ? list : null;
            }
            catch
            {
                return null;
            }
        }

        private static List<PathInfoDTO>? ExtractSensitivePaths(JObject json)
        {
            try
            {
                var scanner = json["sensitivePathScanner"];
                if (scanner == null) return null;

                var results = scanner["results"] as JArray;
                if (results == null || !results.Any()) return null;

                var list = new List<PathInfoDTO>();
                foreach (var item in results)
                {
                    var path = item["path"]?.ToString();
                    var exists = item["exists"]?.Value<bool>() ?? false;
                    var status = item["statusCode"]?.Value<int>() ?? 0;
                    var evidence = item["evidence"]?.ToString();

                    if (path != null && exists)
                    {
                        list.Add(new PathInfoDTO(
                            Caminho: path,
                            StatusCode: status,
                            Resultado: GetStatusDescription(status),
                            Evidencia: evidence
                        ));
                    }
                }
                return list.Any() ? list : null;
            }
            catch
            {
                return null;
            }
        }

        private static List<CookieInfoDTO>? ExtractCookies(JObject json)
        {
            try
            {
                var cookies = json["cookies"] as JArray;
                if (cookies == null || !cookies.Any()) return null;

                var list = new List<CookieInfoDTO>();
                foreach (var cookie in cookies)
                {
                    var nome = cookie["nome"]?.ToString();
                    var temSecure = cookie["temSecure"]?.Value<bool>() ?? false;
                    var temHttpOnly = cookie["temHttpOnly"]?.Value<bool>() ?? false;
                    var sameSite = cookie["sameSite"]?.ToString() ?? "Not set";
                    var risco = cookie["risco"]?.ToString() ?? "Informativo";
                    var descricao = cookie["descricao"]?.ToString() ?? "";

                    if (nome != null)
                    {
                        list.Add(new CookieInfoDTO(
                            Nome: nome,
                            TemSecure: temSecure,
                            TemHttpOnly: temHttpOnly,
                            SameSite: sameSite,
                            Risco: risco,
                            Descricao: descricao
                        ));
                    }
                }
                return list.Any() ? list : null;
            }
            catch
            {
                return null;
            }
        }

        private static List<HttpRedirectInfoDTO>? ExtractHttpRedirects(JObject json)
        {
            try
            {
                var results = json["resultsHttpRedirectScanner"] as JArray;
                if (results == null || !results.Any()) return null;

                var list = new List<HttpRedirectInfoDTO>();
                foreach (var result in results)
                {
                    var ip = result["ip"]?.ToString();
                    var portaAberta = result["port_80_open"]?.Value<bool>() ?? false;
                    var redirectDetectado = result["redirect_detected"]?.Value<bool>() ?? false;
                    var redirectUrl = result["redirect_url"]?.ToString();
                    var severidade = result["severity"]?.ToString();
                    var descricao = result["description"]?.ToString();

                    if (ip != null)
                    {
                        list.Add(new HttpRedirectInfoDTO(
                            IP: ip,
                            PortaAberta: portaAberta,
                            RedirectDetectado: redirectDetectado,
                            RedirectUrl: redirectUrl,
                            Severidade: severidade,
                            Descricao: descricao
                        ));
                    }
                }
                return list.Any() ? list : null;
            }
            catch
            {
                return null;
            }
        }

        private static SslInfoDTO? ExtractSslInfo(JObject json)
        {
            try
            {
                var ssl = json["ssl"];
                if (ssl == null) return null;

                var valido = ssl["isValid"]?.Value<bool>() ?? false;
                var emissor = ssl["issuer"]?.ToString();
                var expiresStr = ssl["expiresOn"]?.ToString();
                DateTime? expira = null;
                if (DateTime.TryParse(expiresStr, out var exp))
                    expira = exp;

                var diasRestantes = ssl["daysUntilExpiration"]?.Value<int>();
                var versao = ssl["protocolVersion"]?.ToString();

                return new SslInfoDTO(
                    Valido: valido,
                    Emissor: emissor,
                    DataExpiracao: expira,
                    DiasRestantes: diasRestantes,
                    VersaoProtocolo: versao
                );
            }
            catch
            {
                return null;
            }
        }

        private static RobotsInfoDTO? ExtractRobotsInfo(JObject json)
        {
            try
            {
                var robotsScanner = json["robotsScanner"];
                if (robotsScanner == null) return null;

                var encontrado = robotsScanner["robots_found"]?.Value<bool>() ?? false;
                var sitemapEncontrado = robotsScanner["sitemap_found"]?.Value<bool>() ?? false;
                var sitemapUrl = robotsScanner["sitemap_url"]?.ToString();
                
                // Extrai alertas
                var alertsArray = robotsScanner["alerts"] as JArray;
                List<RobotsAlertDTO>? alertas = null;
                
                if (alertsArray != null && alertsArray.Any())
                {
                    alertas = new List<RobotsAlertDTO>();
                    foreach (var alert in alertsArray)
                    {
                        var severidade = alert["severity"]?.ToString() ?? "Informativo";
                        var mensagem = alert["message"]?.ToString() ?? "";
                        
                        if (!string.IsNullOrEmpty(mensagem))
                        {
                            alertas.Add(new RobotsAlertDTO(severidade, mensagem));
                        }
                    }
                }

                // Tenta obter conteúdo do robots (se disponível no JSON)
                string? conteudo = null;

                return new RobotsInfoDTO(
                    Encontrado: encontrado,
                    SitemapEncontrado: sitemapEncontrado,
                    SitemapUrl: sitemapUrl,
                    Alertas: alertas,
                    Conteudo: conteudo
                );
            }
            catch
            {
                return null;
            }
        }

        private static string GetServiceName(int port)
        {
            return port switch
            {
                80 => "HTTP",
                443 => "HTTPS",
                21 => "FTP",
                22 => "SSH",
                23 => "Telnet",
                25 => "SMTP",
                53 => "DNS",
                110 => "POP3",
                143 => "IMAP",
                3306 => "MySQL",
                5432 => "PostgreSQL",
                8080 => "HTTP-Alt",
                8443 => "HTTPS-Alt",
                _ => "Desconhecido"
            };
        }

        private static string GetStatusDescription(int statusCode)
        {
            return statusCode switch
            {
                200 => "Acessível",
                301 or 302 => "Redirecionado",
                403 => "Protegido",
                404 => "Não encontrado",
                500 => "Erro servidor",
                _ => $"Status {statusCode}"
            };
        }
    }
}
