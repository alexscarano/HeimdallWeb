using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;

namespace HeimdallWeb.Interfaces;

public interface ILogRepository
{
    public Task<bool> AddLog(LogModel log);
    
    public Task<bool> AddLogImmediate(LogModel log);

    public Task<List<PaginatedResult<LogModel>>> GetLogs(int pageNumber, int pageSize, string? levelFilter, DateTime? startDate, DateTime? endDate);

    public Task<int> GetTotalLogCount(string? levelFilter, DateTime? startDate, DateTime? endDate);

    public Task<LogModel>? GetLog(int log_id);
}
