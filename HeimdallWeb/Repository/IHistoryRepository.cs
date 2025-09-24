using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;

namespace HeimdallWeb.Repository
{
    public interface IHistoryRepository
    {
        Task<PaginatedResult<HistoryModel>> getAllHistories(int page = 1, int pageSize = 10);

        Task<HistoryModel> insertHistory(HistoryModel History);

        Task<HistoryModel?> getHistoryById(int id);

        Task<PaginatedResult<HistoryModel?>> getHistoriesByUserID(int id, int page = 1, int pageSize = 10);

        Task<bool> deleteHistory(int id);
    }
}
