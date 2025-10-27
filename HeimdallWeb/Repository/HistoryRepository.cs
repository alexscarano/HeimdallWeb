using ASHelpers.Extensions;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Repository
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HistoryRepository(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _appDbContext = appDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> deleteHistory(int id)
        {
            var historyToDelete = await getHistoryById(id);

            if (historyToDelete is null)
                return false;

            _appDbContext.Remove(historyToDelete);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<PaginatedResult<HistoryModel>?> getAllHistories(int page = 1 , int pageSize = 10)
        {
            try
            {
                var query = _appDbContext.History.AsQueryable();

                var totalCount = await query.
                    AsNoTracking().
                    CountAsync();

                var items = await query
                 .OrderByDescending(h => h.created_date)
                 .Skip(((page - 1) * pageSize))
                 .Take(pageSize)
                 .AsNoTracking()
                 .ToListAsync();

                return new PaginatedResult<HistoryModel>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception)
            {
                return new PaginatedResult<HistoryModel>();
                throw;
            }
        }

        public async Task<PaginatedResult<HistoryModel?>> getHistoriesByUserID(int id, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _appDbContext.History.AsQueryable();
                int user_id = CookiesHelper.getUserIDFromCookie(CookiesHelper.getAuthCookie(_httpContextAccessor.HttpContext.Request));
                id = user_id;

                query = query.Where(h => h.user_id == id);

                var totalCount = await query.
                    AsNoTracking().
                    CountAsync();

                var items = await query
                 .OrderByDescending(h => h.created_date)
                 .Skip((page - 1) * pageSize)
                 .Take(pageSize)
                 .ToListAsync();

                return new PaginatedResult<HistoryModel?>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception) 
            {
                return new PaginatedResult<HistoryModel?>();
            }
        }

        public async Task<HistoryModel?> getHistoryById(int id)
        {
            var history = await _appDbContext.History
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.history_id == id);

            return history;
        }

        public async Task<JObject> getJsonByHistoryId(int id)
        {
            var history = await getHistoryById(id);

            if (history is null)
                return "{}".ToJson();

            var user_id = CookiesHelper.getUserIDFromCookie(CookiesHelper.getAuthCookie(_httpContextAccessor.HttpContext.Request));

            if (history.user_id != user_id)
                return "{}".ToJson();

            var json = history.raw_json_result;

            if (string.IsNullOrWhiteSpace(json))
                return "{}".ToJson();

            return json.ToJson();
        }

        public async Task<HistoryModel> insertHistory(HistoryModel history)
        {
            int user_id = CookiesHelper.getUserIDFromCookie(CookiesHelper.getAuthCookie(_httpContextAccessor.HttpContext.Request));

            history.user_id = user_id;
            await _appDbContext.History.AddAsync(history);
            await _appDbContext.SaveChangesAsync();

            return history; 
        }
    }
}
