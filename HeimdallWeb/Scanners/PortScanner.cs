using System.Net;
using System.Net.Sockets;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners
{
    public class PortScanner : IScanner
    {
        /// <summary>
        /// Timeout de tentativa de conexão
        /// </summary>
        private readonly TimeSpan _connectTimeout;
        /// <summary>
        /// Timeout de leitura após a conexão
        /// </summary>
        private readonly TimeSpan _readTimeout;

        /// <summary>
        /// Número de tasks paralelas
        /// </summary>
        private readonly int _maxParallel;
        /// <summary>
        /// Pode ser alterado, captura o banner da porta para indentificação posterior
        /// </summary>
        private readonly bool _tryBanner;

        /// <summary>
        /// Pode ser alterado em um input futuramente para selecionar portas especificas.
        /// Aconselhavelmente não um número grande pois o scan pode demorar muito,
        /// porque é criada uma task para cada scan
        /// </summary>
        private readonly List<int> _defaultPorts = new List<int>
        {
            // Web
            80,    // HTTP
            443,   // HTTPS
            8080,  // HTTP-alt / apps
            8443,  // HTTPS-alt / admin panels

            // FTP / SFTP (upload sites / deploy)
            20,    // FTP-data (opcional, mas relacionado)
            21,    // FTP
            22,    // SSH / SFTP

            // Email (servidores que sites frequentemente interagem)
            25,    // SMTP
            465,   // SMTPS
            587,   // SMTP submission
            110,   // POP3
            995,   // POP3S
            143,   // IMAP
            993,   // IMAPS

            // DNS (importante para sites)
            53,    // DNS

            // Bancos de dados comuns em hosting de sites
            3306,  // MySQL / MariaDB
            5432,  // PostgreSQL
            27017, // MongoDB (alguns sites/serviços)
            1433, // SQL Server
            1521, // Oracle DB

            // Cache / session / auxiliar web
            6379,  // Redis
            11211, // Memcached

            // Painéis e webmail / painéis de hospedagem (comuns em hosts compartilhados)
            2082,  // cPanel (non-SSL)
            2083,  // cPanel (SSL)
            2095,  // Webmail
            2096,  // Webmail (SSL)

            // RDP (às vezes exposto em servidores windows)
            3389   // RDP
        };


        /// <summary>
        /// O construtor com parâmetros 
        /// </summary>
        /// <param name="connectTimeout"></param>
        /// <param name="readTimeout"></param>
        /// <param name="maxParallel"></param>
        /// <param name="tryBanner"></param>
        public PortScanner(TimeSpan? connectTimeout = null, TimeSpan? readTimeout = null, int maxParallel = 30, bool tryBanner = true)
        {
            _connectTimeout = connectTimeout ?? TimeSpan.FromSeconds(3);
            _readTimeout = readTimeout ?? TimeSpan.FromSeconds(2);
            _maxParallel = Math.Max(1, maxParallel);
            _tryBanner = tryBanner;
        }

        /// <summary>
        /// Realiza o scan assíncrono de portas 
        /// </summary>
        /// <param name="targetRaw"></param>
        /// <returns>Retorna o json com dados do scan</returns>
        public async Task<JObject> scanAsync(string targetRaw, CancellationToken cancellationToken = default/*,List<int>? ports = null*/)
        {
            try
            {
                //ports ??= _defaultPorts;

                IPAddress[] target = NetworkUtils.GetIPv4Addresses(targetRaw);

                var results = new JArray();
                using var sem = new SemaphoreSlim(_maxParallel);
                var probeTasks = new List<Task>();

                foreach (var ip in target)
                {
                    foreach (var port in _defaultPorts.Distinct())
                    {
                        probeTasks.Add(Task.Run(async () =>
                        {
                            await sem.WaitAsync(cancellationToken);
                            try
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                var probe = await ProbeIpPortAsync(ip, port, cancellationToken);
                                lock (results)
                                {
                                    results.Add(probe);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                throw;
                            }
                            catch { /* Ignorar erros de conexão */ }
                            finally
                            {
                                sem.Release();
                            }
                        }, cancellationToken));
                    }
                }

                await Task.WhenAll(probeTasks);

                var output = new JObject
                {
                    ["target"] = targetRaw,
                    ["ips"] = new JArray(target.Select(i => i.ToString())),
                    ["scanTime"] = DateTime.Now,
                    ["resultsPortScanner"] = results
                };

                return output;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                var results_error = new JArray();
                var output_error = new JObject
                {
                    ["error"] = "failed_to_resolve_or_scan",
                    ["scanner"] = "PortScanner",
                    ["target"] = targetRaw,
                    ["scanTime"] = DateTime.Now,
                    ["resultsPortScanner"] = results_error
                };

                return output_error;
            }
        }

        /// <summary>
        /// Captura dados de cada porta escaneada pelo scan principal.
        /// Utilizado como método auxiliar
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private async Task<JObject> ProbeIpPortAsync(IPAddress ip, int port, CancellationToken cancellationToken = default)
        {
            var probe = new JObject
            {
                ["ip"] = ip.ToString(),
                ["port"] = port,
                ["scanTime"] = DateTime.Now
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_connectTimeout);
            try
            {
                using var tcp = new TcpClient(ip.AddressFamily);
                var connectTask = tcp.ConnectAsync(ip, port);
                var finished = await Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, cts.Token));

                if (!tcp.Connected)
                {
                    probe["open"] = false;
                    probe["error"] = "timeout_or_closed";
                    return probe;
                }

                probe["open"] = true;

                // tentar banner grab (leitura curta) se configurado, true or false nas propriedades
                if (_tryBanner)
                {
                    try
                    {
                        tcp.Client.ReceiveTimeout = (int)_readTimeout.TotalMilliseconds;
                        using var stream = tcp.GetStream();

                        // se porta 80/8080/443/8443, você pode enviar requisição HTTP simples para provocar banner
                        if (port == 80 || port == 8080)
                        {
                            var req = $"HEAD / HTTP/1.1\r\nHost: {ip}\r\nConnection: close\r\n\r\n";
                            var data = Encoding.ASCII.GetBytes(req);
                            await stream.WriteAsync(data, 0, data.Length, cancellationToken);
                        }
                        else if (port == 443 || port == 8443)
                        {
                            // não tenta TLS handshake aqui (o SslScanner faz isso).
                            // Podemos tentar fazer um handshake? evitamos aqui para não duplicar.
                        }

                        // lê até 1KB do banner
                        var buffer = new byte[1024];
                        int read = 0;
                        
                        var readTask = stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        var readFinished = await Task.WhenAny(readTask, Task.Delay(_readTimeout, cancellationToken));
                        if (readFinished == readTask)
                            read = readTask.Result;

                        if (read > 0)
                        {
                            var banner = Encoding.UTF8.GetString(buffer, 0, read).Trim();
                            probe["banner"] = banner.Length > 512 ? banner.Substring(0, 512) : banner;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception exBanner)
                    {
                        probe["banner_error"] = exBanner.Message;
                    }
                }

                return probe;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                probe["open"] = false;
                probe["error"] = ex.Message;
                return probe;
            }
        }
    }
}
