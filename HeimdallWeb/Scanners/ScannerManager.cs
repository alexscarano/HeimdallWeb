using HeimdallWeb.Interfaces;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners
{
    public class ScannerManager
    {
        private readonly IList<IScanner> _scanners = new List<IScanner>();

        public ScannerManager()
        {
            _scanners.Add(new HeaderScanner());
            _scanners.Add(new SslScanner());
            _scanners.Add(new PortScanner());
            _scanners.Add(new HttpRedirectScanner());
            _scanners.Add(new RobotsScanner());
        }

        public async Task<JObject> RunAllAsync(string target, CancellationToken cancellationToken = default)
        {
            JObject results = new();
            foreach (var scanner in _scanners)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await scanner.ScanAsync(target, cancellationToken);

                results.Merge(result, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }
            return results;
        }

    }
}
