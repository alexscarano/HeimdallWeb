using HeimdallWeb.Helpers;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners
{
    public class HeaderScanner : IScanner
    {
        private readonly Dictionary<string, Func<string, bool>> _securityHeaders = new()
        {
            { "Strict-Transport-Security", v => v.Contains("max-age") },
            { "Content-Security-Policy", v => !string.IsNullOrWhiteSpace(v) },
            { "X-Frame-Options", v => v.Equals("DENY", StringComparison.OrdinalIgnoreCase) ||
                                      v.Equals("SAMEORIGIN", StringComparison.OrdinalIgnoreCase) },
            { "X-Content-Type-Options", v => v.Equals("nosniff", StringComparison.OrdinalIgnoreCase) },
            { "Referrer-Policy", v => !v.Equals("unsafe-url", StringComparison.OrdinalIgnoreCase) },
            { "Permissions-Policy", v => !string.IsNullOrWhiteSpace(v) },
            { "Cache-Control", v => v.Contains("no-store") || v.Contains("no-cache") },
        };

        public async Task<JObject> scanAsync(string targetRaw)
        {
            try
            {
                using var client = new HttpClient();
                string target = await NetworkUtils.NormalizeUrl(targetRaw);

                var response = await client.GetAsync(target);
     
                var headers = response.Headers
                    .ToDictionary(h => h.Key, h => string.Join(";", h.Value));

                var contentHeaders = response.Content.Headers
                    .ToDictionary(h => h.Key, h => string.Join(";", h.Value));

                var allHeaders = headers.Concat(contentHeaders)
                                        .ToDictionary(h => h.Key, h => h.Value);


                var present = new Dictionary<string, string>();
                var weak = new Dictionary<string, string>();
                var missing = new List<string>();

                foreach (var secHeader in _securityHeaders)
                {
                    if (headers.TryGetValue(secHeader.Key, out string keyValue))
                    {
                        if (secHeader.Value(keyValue))
                            present[secHeader.Key] = keyValue;
                        else
                            weak[secHeader.Key] = keyValue;
                    }
                    else
                    {
                        missing.Add(secHeader.Key);
                    }
                }

                return JObject.FromObject(new
                {
                    statusCodeHttpRequest = (int)response.StatusCode,
                    headers = allHeaders,
                    securityHeaders = new
                    {
                        present,
                        weak,
                        missing
                    },
                    scanTime = DateTime.Now
                });

            }
            catch (Exception)
            {
                return new JObject();
            }
        }
    }
}
