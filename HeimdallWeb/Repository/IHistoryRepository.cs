using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public interface IHistoryRepository
    {
        Task<List<HistoryModel>> getAllHistories();

        Task<HistoryModel> insertHistory(HistoryModel History);

        Task<HistoryModel?> getHistoryById(int id);

        Task<bool> deleteHistory(int id);
    }
}
