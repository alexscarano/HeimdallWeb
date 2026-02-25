using System.Net;
using System.Net.Sockets;
using System.Text;
using HeimdallWeb.Application.Helpers;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Application.Services.Scanners
{
    public class HttpRedirectScanner : IScanner
    {
        public ScannerMetadata Metadata => new(
            Key: "Redirect",
            DisplayName: "HTTP Redirect",
            Category: "Redirect",
            DefaultTimeout: TimeSpan.FromSeconds(8));

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
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(_connectTimeout + _readTimeout);

                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };

                using var client = new HttpClient(handler)
                {
                    Timeout = _connectTimeout + _readTimeout
                };

                // Request using the IP address directly
                var requestUrl = $"http://{ip}/";

                try
                {
                    var response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token);

                    probe["port_80_open"] = true;
                    var statusCode = (int)response.StatusCode;

                    if (statusCode >= 300 && statusCode < 400)
                    {
                        probe["redirect_detected"] = true;

                        if (statusCode == 301 || statusCode == 308)
                            probe["redirect_type"] = "permanent";
                        else if (statusCode == 302 || statusCode == 307)
                            probe["redirect_type"] = "temporary";

                        var location = response.Headers.Location?.ToString() ?? "";

                        if (!string.IsNullOrEmpty(location))
                        {
                            probe["redirect_url"] = location;
                            probe["status"] = "redirect_found";

                            if (location.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                probe["severity"] = "Informativo";
                                probe["description"] = "Redirect HTTP → HTTPS configurado corretamente";
                            }
                            else if (location.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                            {
                                probe["severity"] = "Alto";
                                probe["description"] = "Redirect HTTP → HTTP (não seguro)";
                            }
                            else
                            {
                                probe["severity"] = "Baixo";
                                probe["description"] = $"Redirect configurado para: {location}";
                            }
                        }
                    }
                    else if (statusCode == 400)
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
                }
                catch (HttpRequestException)
                {
                    probe["status"] = "connection_failed";
                    probe["port_80_open"] = false;
                }
                catch (TaskCanceledException)
                {
                    probe["status"] = "connection_failed";
                    probe["port_80_open"] = false;
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
