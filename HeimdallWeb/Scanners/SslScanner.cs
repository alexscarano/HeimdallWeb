using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeimdallWeb.Scanners
{
    public class SslScanner : IScanner
    {
        private readonly List<int> _ports = new() { 443 };

        public async Task<JObject> ScanAsync(string targetRaw, CancellationToken cancellationToken = default)
        {
            try
            {
                var target = NetworkUtils.RemoveHttpString(targetRaw);
                var results = new JArray();

                Console.WriteLine("[SslScanner] Verificando certificado SSL/TLS");

                    foreach (var port in _ports)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            using var tcpClient = new TcpClient();
                            var connectTask = tcpClient.ConnectAsync(target, port);
                            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                            cts.CancelAfter(TimeSpan.FromSeconds(5));
                            var completedTask = await Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, cts.Token));

                            if (!tcpClient.Connected)
                            {
                                results.Add(JObject.FromObject(new
                                {
                                    port,
                                    reachable = false,
                                    error = "Não conectado (timeout ou porta fechada)"
                                }));
                                continue;
                            }

                            using var sslStream = new SslStream(tcpClient.GetStream(), false,
                                (sender, certificate, chain, sslPolicyErrors) => true);

                            var authTask = sslStream.AuthenticateAsClientAsync(target);
                            await authTask.WaitAsync(cancellationToken);

                            var remoteCert = sslStream.RemoteCertificate;
                            var cert2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(remoteCert);

                            var chain2 = new System.Security.Cryptography.X509Certificates.X509Chain();
                            chain2.ChainPolicy.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.Online;
                            var chainIsValid = chain2.Build(cert2);

                            int daysToExpire = (cert2.NotAfter - DateTime.Now).Days;
                            bool expired = cert2.NotAfter < DateTime.Now;
                            
                            var sigAlg = cert2.SignatureAlgorithm.FriendlyName ?? cert2.SignatureAlgorithm.Value;
      
                            var rsa = cert2.GetRSAPublicKey();
                            var dsa = cert2.GetDSAPublicKey();
                            var ecdsa = cert2.GetECDsaPublicKey();
                            int keySize = 0;
                            if (rsa != null)
                                keySize = rsa.KeySize;
                            else if (dsa != null)
                                keySize = dsa.KeySize;
                            else if (ecdsa != null)
                                keySize = ecdsa.KeySize;

                        // Certificados com SHA-1 ou MD5 são considerados inseguros
                        var usesWeakSig = !string.IsNullOrEmpty(sigAlg) && (
                                sigAlg.Contains("sha1", StringComparison.OrdinalIgnoreCase) ||
                                sigAlg.Contains("md5", StringComparison.OrdinalIgnoreCase)
                            );

                            // Determina severidade baseado no status do certificado
                            string severity;
                            string description;
                            
                            if (expired)
                            {
                                severity = "Critico";
                                description = $"Certificado SSL expirado há {Math.Abs(daysToExpire)} dias";
                            }
                            else if (daysToExpire < 0)
                            {
                                severity = "Critico";
                                description = "Certificado SSL expirado";
                            }
                            else if (daysToExpire <= 7)
                            {
                                severity = "Alto";
                                description = $"Certificado SSL expira em {daysToExpire} dias (crítico)";
                            }
                            else if (daysToExpire <= 30)
                            {
                                severity = "Medio";
                                description = $"Certificado SSL expira em {daysToExpire} dias (atenção necessária)";
                            }
                            else if (usesWeakSig)
                            {
                                severity = "Alto";
                                description = $"Certificado SSL usa algoritmo fraco ({sigAlg})";
                            }
                            else if (!chainIsValid)
                            {
                                severity = "Alto";
                                description = "Cadeia de certificado SSL inválida";
                            }
                            else if (keySize < 2048 && rsa != null)
                            {
                                severity = "Medio";
                                description = $"Chave RSA pequena ({keySize} bits, recomendado >= 2048)";
                            }
                            else if (daysToExpire <= 90)
                            {
                                severity = "Informativo";
                                description = $"Certificado SSL válido, expira em {daysToExpire} dias (considere renovação em breve)";
                            }
                            else
                            {
                                severity = "Informativo";
                                description = $"Certificado SSL válido e configurado corretamente (expira em {daysToExpire} dias)";
                            }

                            var certObj = new JObject
                            {
                                ["port"] = port,
                                ["reachable"] = true,
                                ["subject"] = cert2.Subject,
                                ["issuer"] = cert2.Issuer,
                                ["notBefore"] = cert2.NotBefore,
                                ["notAfter"] = cert2.NotAfter,
                                ["expired"] = expired,
                                ["daysToExpire"] = daysToExpire,
                                ["thumbprint"] = cert2.Thumbprint,
                                ["signatureAlgorithm"] = sigAlg,
                                ["publicKeySize"] = keySize,
                                ["chainValid"] = chainIsValid,
                                ["chainStatus"] = new JArray(chain2.ChainStatus.Select(s => s.StatusInformation.Trim()).Where(s => !string.IsNullOrEmpty(s))),
                                ["weakSignatureAlgorithm"] = usesWeakSig,
                                ["severity"] = severity,
                                ["description"] = description
                            };
                            results.Add(certObj);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception exPort)
                        {
                            results.Add(JObject.FromObject(new
                            {
                                port,
                                reachable = false,
                                error = exPort.Message
                            }));
                        }

                        return JObject.FromObject(new
                        {
                            scanTime = DateTime.Now,
                            resultsSslScanner = results
                        });
                    }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return JObject.FromObject(new
                {
                    scanner = "SslScanner",
                    error = ex.Message
                });
            }           
            return new (JObject.FromObject(new
            {
                port = _ports,
                reachable = false,
                error = "Não conectado (timeout ou porta fechada)"
            })); 
        }
    }
}
