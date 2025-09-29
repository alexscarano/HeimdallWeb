using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public interface IFindingRepository
    {
        public Task<FindingModel> insertFinding(FindingModel finding);
        public Task SaveFindingsFromIAAsync(string iaResponse, int historyId);
        public Task<List<FindingModel>> getFindingsByHistoryId(int historyId);
    }
}
