using HeimdallWeb.Models;

namespace HeimdallWeb.Interfaces
{
    public interface IFindingRepository
    {
        public Task SaveFindingsFromAI(string iaResponse, int historyId);
        public Task<List<FindingModel>> getFindingsByHistoryId(int historyId);
    }
}
