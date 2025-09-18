using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners
{
    public class PortScanner : IScanner
    {
        public Task<JObject> scanAsync(string target)
        {
            throw new NotImplementedException();
        }
    }
}
