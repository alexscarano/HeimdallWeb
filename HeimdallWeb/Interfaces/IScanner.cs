using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Interfaces
{
    public interface IScanner
    {
        Task<JObject> scanAsync(string target);
    
    }
}
