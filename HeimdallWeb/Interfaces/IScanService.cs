using HeimdallWeb.Models;

namespace HeimdallWeb.Interfaces
{
    public interface IScanService
    {
        public Task<int> RunScanAndPersist(string domainRaw, HistoryModel historyModel,CancellationToken cancellationToken = default);
    }
}
