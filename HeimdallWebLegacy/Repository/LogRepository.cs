using HeimdallWeb.Enums;
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

    public async Task<bool> AddLog(LogModel entry)
    {
        try
        {   
            if (entry is null)
                return false;

            var model = new LogModel
            {
                code = entry.code, 
                timestamp = DateTime.Now,
                level = GetLevel(entry.code),
                source = entry.source ?? "SYSTEM",
                message = entry.message, 
                details = entry.details,
                user_id = entry.user_id,
                history_id = entry.history_id,
                remote_ip = entry.remote_ip
            };

            await _dbContext.Log.AddAsync(model);
            // SaveChangesAsync será chamado externamente (ou em batch)
            // Importante: logs fora de transação devem usar AddLogImmediate

            return true;
        }
        catch 
        {
            return false;
        }
    }

    // Método para logs que precisam ser persistidos imediatamente (fora de transação)
    public async Task<bool> AddLogImmediate(LogModel entry)
    {
        try
        {   
            if (entry is null)
                return false;

            var model = new LogModel
            {
                code = entry.code, 
                timestamp = DateTime.Now,
                level = GetLevel(entry.code),
                source = entry.source ?? "SYSTEM",
                message = entry.message, 
                details = entry.details,
                user_id = entry.user_id,
                history_id = entry.history_id,
                remote_ip = entry.remote_ip
            };

            await _dbContext.Log.AddAsync(model);
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
            {
                var s = startDate.Value.Date;
                query = query.Where(l => l.timestamp >= s);
            }

            if (endDate.HasValue)
            {
                // Treat endDate as inclusive for the whole day by using an exclusive upper bound
                var eExclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(l => l.timestamp < eExclusive);
            }

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
            {
                var s = startDate.Value.Date;
                query = query.Where(l => l.timestamp >= s);
            }

            if (endDate.HasValue)
            {
                var eExclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(l => l.timestamp < eExclusive);
            }

            return await query.CountAsync();
        }
        catch
        {

            return 0;
        }
    }

    private string GetLevel(LogEventCode code)
    {
        return code switch
        {
            LogEventCode.SCAN_ERROR => "ERROR",
            LogEventCode.AI_RESPONSE_ERROR => "ERROR",
            LogEventCode.DB_SAVE_ERROR => "ERROR",
            LogEventCode.UNHANDLED_EXCEPTION => "CRITICAL",
            LogEventCode.USER_LOGIN_FAILED => "WARNING",
            LogEventCode.PATH_SUSPECTED_FALLBACK => "WARNING",
            _ => "INFO"
        };
    }
}
