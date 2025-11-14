using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;

namespace HeimdallWeb.Repository;

public class LogRepository : ILogRepository
{
    private readonly AppDbContext _dbContext;

    public LogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddLog(LogModel log)
    {
        try
        {   
            if (log is null)
                return false;

            await _dbContext.Log.AddAsync(log);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch 
        {
            return false;
        }
    }

    public async Task<LogModel>? GetLog(int log)
    {
        try
        {
            if (log <= 0)
                return null!;

            var result = await _dbContext.Log.FindAsync(log);

            if (result is null)
                return null!;

            return result;
        }
        catch 
        {
            return null!;
        }
    }

    public async Task<List<PaginatedResult<LogModel>>> GetLogs(int pageNumber, int pageSize, string? levelFilter, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<LogModel> query = _dbContext.Log.AsQueryable();

            if (!string.IsNullOrWhiteSpace(levelFilter))
                query = query.Where(l => l.level == levelFilter);

            if (startDate.HasValue)
                query = query.Where(l => l.timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.timestamp <= endDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginated = new PaginatedResult<LogModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageNumber,
                PageSize = pageSize,
            };

            return new List<PaginatedResult<LogModel>> { paginated };
        }
        catch
        {
            return new List<PaginatedResult<LogModel>>();
        }
    }

    public async Task<int> GetTotalLogCount(string? levelFilter, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            IQueryable<LogModel> query = _dbContext.Log.AsQueryable();

            if (!string.IsNullOrWhiteSpace(levelFilter))
                query = query.Where(l => l.level == levelFilter);

            if (startDate.HasValue)
                query = query.Where(l => l.timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.timestamp <= endDate.Value);

            return await query.CountAsync();
        }
        catch
        {

            return 0;
        }
    }

}
