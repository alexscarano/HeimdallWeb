using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;

namespace HeimdallWeb.Repository
{
    public interface IHistoryRepository
    {
        Task<PaginatedResult<HistoryModel>> getAllHistories(string? where, int page, int pageSize);

        Task<HistoryModel> insertHistory(HistoryModel History);

        Task<HistoryModel?> getHistoryById(int id);

        Task<PaginatedResult<HistoryModel?>> getHistoriesByUserID(int id);

        Task<bool> deleteHistory(int id);
    }
}
