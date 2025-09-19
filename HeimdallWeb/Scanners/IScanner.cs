using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Scanners
{
    public interface IScanner
    {
        Task<JObject> scanAsync(string target);

        //Task<JObject> scanAsync(string target, List<int>? ports = null);
    }
}
