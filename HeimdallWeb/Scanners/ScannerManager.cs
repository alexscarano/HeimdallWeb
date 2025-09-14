using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners
{
    public class ScannerManager
    {
        private readonly IList<IScanner> _scanners = new List<IScanner>();

        public ScannerManager()
        {
            _scanners.Add(new HeaderScanner());
        }

        public async Task<JObject> RunAllAsync(string target)
        {
            JObject results = new();
            foreach (var scanner in _scanners)
            {
                var result = await scanner.scanAsync(target);

                results.Merge(result, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }
            return results;
        }

    }
}
