using System.Net;
using Microsoft.Extensions.Hosting;

namespace HeimdallWeb.Helpers
{
    public static class NetworkUtils
    {
        /// <summary>
        /// Normalização de URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async static Task<string> NormalizeUrl(string url)
        {
			try
			{
                if (string.IsNullOrEmpty(url))
                {
                    throw new ArgumentException("A url não pode estar vazia");
                }

                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }

                if (!IsValidUrl(url, out Uri? uriResult))
                    throw new ArgumentException("O alvo informado não é uma URL válida.");

                //Testa se é acessível, se não for tenta com http: 
                if (!await IsReachableAsync(url) && uriResult!.Scheme == Uri.UriSchemeHttps)
                {
                    string fallback = url.Replace("https://", "http://");
                    if (await IsReachableAsync(fallback))
                        return fallback;
                }

                return uriResult!.ToString();
            }
			catch (Exception)
			{
                return string.Empty;
            }
        }

        /// <summary>
        /// Validação de formato (sintaxe)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uriResult"></param>
        /// <returns></returns>
        public static bool IsValidUrl(string url, out Uri? uriResult)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Validação de acessibilidade (rede)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<bool> IsReachableAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("A url não pode estar vazia");
            }

            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                return response.IsSuccessStatusCode; // 200–299
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove o http/https do início da URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string removeHttpString(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("A url não pode estar vazia");
            }

            if (url.StartsWith("http://"))
            {
                url = url.Replace("http://", "");
            }
            else if (url.StartsWith("https://"))
            {
                url = url.Replace("https://", "");
            }
            
            if (url.Contains('/'))
            {
                url = url.Substring(0, url.IndexOf('/'));//Remove tudo após a primeira barra
            }

            return url;
        }

        /// <summary>
        /// Capturar o endereço ipv4 com base no DNS
        /// </summary>
        /// <param name="rawHost"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IPAddress[] GetIPv4Addresses(string rawHost)
        {
            List<IPAddress> ipv4Addresses = new List<IPAddress>();

            try
            {
                var host = removeHttpString(rawHost);
                var addresses = Dns.GetHostAddresses(host);
                foreach (var addr in addresses)
                {
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipv4Addresses.Add(addr);
                        return ipv4Addresses.ToArray();
                    }
                }
                throw new Exception("Nenhum ipv4 foi achado para esse host.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em resolver o '{rawHost}': {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se a string é um endereço IP válido
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsIPAddress(string input)
        {
            return IPAddress.TryParse(input, out _);
        }

    }
}
