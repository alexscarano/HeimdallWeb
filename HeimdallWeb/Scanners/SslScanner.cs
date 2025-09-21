using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using HeimdallWeb.Helpers;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners
{
    public class SslScanner : IScanner
    {
        private readonly List<int> _ports = new() { 443 };

        public async Task<JObject> scanAsync(string targetRaw)
        {
            try
            {
                var target = NetworkUtils.removeHttpString(targetRaw);
                var results = new JArray();

                    foreach (var port in _ports)
                    {
                        try
                        {
                            using var tcpClient = new TcpClient();
                            var connectTask = tcpClient.ConnectAsync(target, port);
                            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                            var completedTask = Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, cts.Token)).Result;

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

                            await sslStream.AuthenticateAsClientAsync(target);

                            var remoteCert = sslStream.RemoteCertificate;
                            var cert2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(remoteCert);

                            var chain2 = new System.Security.Cryptography.X509Certificates.X509Chain();
                            chain2.ChainPolicy.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.Online;
                            var chainIsValid = chain2.Build(cert2);

                            int daysToExpire = (cert2.NotAfter - DateTime.UtcNow).Days;
                            bool expired = cert2.NotAfter < DateTime.UtcNow;
                            
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
                                ["weakSignatureAlgorithm"] = usesWeakSig
                            };
                            results.Add(certObj);
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
                            scanTime = DateTime.UtcNow,
                            resultsSslScanner = results
                        });
                    }
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
