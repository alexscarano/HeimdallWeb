using System.Net;
using System.Net.Sockets;
using System.Text;
using HeimdallWeb.Application.Helpers;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners
{
    public class HttpRedirectScanner : IScanner
    {
        private readonly TimeSpan _connectTimeout;
        private readonly TimeSpan _readTimeout;
        private readonly int _maxParallel;

        /// <summary>
        /// Construtor do scanner de redirecionamento HTTP->HTTPS
        /// </summary>
        /// <param name="connectTimeout">Tempo máximo para estabelecer conexão</param>
        /// <param name="readTimeout">Tempo máximo para ler resposta</param>
        /// <param name="maxParallel">Número máximo de verificações paralelas</param>
        public HttpRedirectScanner(
            TimeSpan? connectTimeout = null, 
            TimeSpan? readTimeout = null, 
            int maxParallel = 20)
        {
            _connectTimeout = connectTimeout ?? TimeSpan.FromSeconds(3);
            _readTimeout = readTimeout ?? TimeSpan.FromSeconds(2);
            _maxParallel = Math.Max(1, maxParallel);
        }

        /// <summary>
        /// Realiza o scan assíncrono em busca de redirecionamentos HTTP->HTTPS
        /// </summary>
        /// <param name="targetRaw">Alvo a ser escaneado (domínio ou IP)</param>
        /// <returns>Resultado do scan em formato JSON</returns>
        public async Task<JObject> ScanAsync(string targetRaw, CancellationToken cancellationToken = default)
        {
            try
            {
                var target = NetworkUtils.GetIPv4Addresses(targetRaw);
                var results = new JArray();

                using var sem = new SemaphoreSlim(_maxParallel);
                var probeTasks = new List<Task>();

                foreach (var ip in target)
                {
                    probeTasks.Add(Task.Run(async () =>
                    {
                        await sem.WaitAsync(cancellationToken);
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var probeResult = await ProbeHttpRedirectAsync(ip, cancellationToken);
                            lock (results)
                            {
                                results.Add(probeResult);
                            }
                        }
                        finally
                        {
                            sem.Release();
                        }
                    }, cancellationToken));
                }

                await Task.WhenAll(probeTasks);

                return JObject.FromObject(new 
                {
                    target = targetRaw,
                    ips = new JArray(target.Select(i => i.ToString())),
                    scanTime = DateTime.Now,
                    resultsHttpRedirectScanner = results
                });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return JObject.FromObject(new
                {
                    error = "failed_to_scan",
                    target = targetRaw,
                    scanTime = DateTime.Now,
                    errorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Verifica se existe redirecionamento HTTP->HTTPS para um determinado IP
        /// </summary>
        private async Task<JObject> ProbeHttpRedirectAsync(IPAddress ip, CancellationToken cancellationToken = default)
        {
            var probe = new JObject
            {
                ["ip"] = ip.ToString(),
                ["scanTime"] = DateTime.Now
            };

            try
            {
                using var tcpClient = new TcpClient(ip.AddressFamily);
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(_connectTimeout);

                // Conecta na porta 80 (HTTP)
                var connectTask = tcpClient.ConnectAsync(ip, 80);
                var finished = await Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, cts.Token));

                if (!tcpClient.Connected)
                {
                    probe["status"] = "connection_failed";
                    probe["port_80_open"] = false;
                    return probe;
                }

                probe["port_80_open"] = true;

                // Envia requisição HTTP simples
                using var stream = tcpClient.GetStream();
                stream.ReadTimeout = (int)_readTimeout.TotalMilliseconds;

                var request = $"GET / HTTP/1.1\r\nHost: {ip}\r\nConnection: close\r\n\r\n";
                var requestBytes = Encoding.ASCII.GetBytes(request);
                await stream.WriteAsync(requestBytes, 0, requestBytes.Length, cancellationToken);

                // Lê a resposta até encontrar os headers
                var buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Analisa o tipo de resposta
                if (response.Contains("301") || 
                    response.Contains("302") || 
                    response.Contains("307") ||
                    response.Contains("308")
                    )
                {
                    probe["redirect_detected"] = true;

                    if (response.Contains("301") || response.Contains("308"))
                        probe["redirect_type"] = "permanent";
                    else if (response.Contains("302") || response.Contains("307"))
                        probe["redirect_type"] = "temporary";

                    // Extrai a URL de destino se possível
                    var locationIndex = response.IndexOf("Location:");
                    if (locationIndex != -1)
                    {
                        var locationEnd = response.IndexOf("\r\n", locationIndex);
                        if (locationEnd != -1)
                        {
                            var redirectUrl = response.Substring(locationIndex + 10, locationEnd - locationIndex - 10).Trim();
                            probe["redirect_url"] = redirectUrl;
                            probe["status"] = "redirect_found";
                            
                            // Determina severidade do redirect
                            if (redirectUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                probe["severity"] = "Informativo";
                                probe["description"] = "Redirect HTTP → HTTPS configurado corretamente";
                            }
                            else if (redirectUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                            {
                                probe["severity"] = "Alto";
                                probe["description"] = "Redirect HTTP → HTTP (não seguro)";
                            }
                            else
                            {
                                probe["severity"] = "Baixo";
                                probe["description"] = $"Redirect configurado para: {redirectUrl}";
                            }
                            /*
                                 Os primeiros 10 caracteres são "Location: "
                                 A URL de redirecionamento começa no índice 10
                                 O final da URL termina antes de "\r\n"
                             */
                        }
                    }
                }
                else if (response.Contains("400"))
                {
                    probe["status"] = "bad_request";
                    probe["redirect_detected"] = false;
                    probe["severity"] = "Baixo";
                    probe["description"] = "Servidor respondeu com erro 400 (Bad Request)";
                }
                else
                {
                    probe["redirect_detected"] = false;
                    probe["status"] = "no_redirect";
                    probe["severity"] = "Medio";
                    probe["description"] = "HTTP habilitado sem redirect para HTTPS - considere implementar redirect";
                }

                return probe;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                probe["error"] = ex.Message;
                probe["status"] = "scan_error";
                return probe;
            }
        }
    }
}
