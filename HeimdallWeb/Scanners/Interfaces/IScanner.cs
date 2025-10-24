using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners.Interfaces
{
    public interface IScanner
    {
        Task<JObject> scanAsync(string target);
    
    }
}
