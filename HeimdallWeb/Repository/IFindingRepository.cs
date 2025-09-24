using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public interface IFindingRepository
    {
        public Task<FindingModel> insertFinding(FindingModel finding);

        public Task SaveFindingsFromIAAsync(string iaResponse, int historyId);
    }
}
