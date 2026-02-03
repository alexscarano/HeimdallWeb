using HeimdallWeb.Models;

namespace HeimdallWeb.Interfaces
{
    public interface ITechnologyRepository
    {
        public Task SaveTechnologiesFromAI(string iaResponse,int historyId);
        public Task<List<TechnologyModel>> getTechnologiesByHistoryId(int historyId);
    }
}
