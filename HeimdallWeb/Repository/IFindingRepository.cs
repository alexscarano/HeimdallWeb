using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public interface IFindingRepository
    {
        public Task SaveFindingsFromIA(string iaResponse, int historyId);
        public Task<List<FindingModel>> getFindingsByHistoryId(int historyId);
    }
}
