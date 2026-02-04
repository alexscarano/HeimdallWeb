using Newtonsoft.Json.Linq;
using System.Threading;

namespace HeimdallWeb.Interfaces
{
    public interface IScanner
    {
        Task<JObject> ScanAsync(string target, CancellationToken cancellationToken = default);
    }
}
