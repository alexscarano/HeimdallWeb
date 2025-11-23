using ASHelpers.Extensions;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Enums;
using HeimdallWeb.Models.Map;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Repository
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepository;

        public HistoryRepository(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor, ILogRepository logRepository)
        {
            _appDbContext = appDbContext;
            _httpContextAccessor = httpContextAccessor;
            _logRepository = logRepository;
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
                var query = _appDbContext.History.Where(h => h.has_completed == true).AsQueryable();

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
            catch (Exception ex)
            {
                await _logRepository.AddLog(new LogModel
                {
                    code = LogEventCode.DB_SAVE_ERROR,
                    message = "Erro ao salvar dados no banco",
                    source = "HistoryRepository",
                    details = ex.ToString(),
                    remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
                });
                return new PaginatedResult<HistoryModel>();
                throw;
            }
        }

        public async Task<PaginatedResult<HistoryModel?>> getHistoriesByUserID(int id, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _appDbContext.History.Where(h => h.has_completed == true).AsQueryable();
                int user_id = CookiesHelper.getUserIDFromCookie(CookiesHelper.getAuthCookie(_httpContextAccessor.HttpContext.Request));
                id = user_id;

                // Verificar se usuário está bloqueado
                var user = await _appDbContext.User.FirstOrDefaultAsync(u => u.user_id == id);

                if (user != null && !user.is_active)
                {
                    return new PaginatedResult<HistoryModel?>
                    {
                        Items = new List<HistoryModel?>(),
                        TotalCount = 0,
                        Page = page,
                        PageSize = pageSize
                    };
                }

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

        // Método original: não realiza eager-loading dos relacionamentos
        public async Task<HistoryModel?> getHistoryById(int id)
        {
            var history = await _appDbContext.History
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.history_id == id);

            return history;
        }

        // Novo método que carrega as entidades relacionadas (Findings e Technologies) sem paginação
        public async Task<HistoryModel?> getHistoryByIdWithIncludes(int id)
        {
            var history = await _appDbContext.History
                .AsNoTracking()
                .Include(h => h.Findings)
                .Include(h => h.Technologies)
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

            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.DB_SAVE_OK,
                message = "Registro salvo com sucesso",
                source = "HistoryRepository",
                user_id = user_id,
                history_id = history.history_id,
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
            });

            return history; 
        }

        public async Task<PaginatedResult<HistoryModel>?> getAllHistoriesWithIncludes(int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _appDbContext.History
                    .Where(h => h.has_completed == true)
                    .Include(h => h.Findings)
                    .Include(h => h.Technologies)
                    .AsQueryable();

                var totalCount = await query
                    .AsNoTracking()
                    .CountAsync();

                var items = await query
                    .OrderByDescending(h => h.created_date)
                    .Skip((page - 1) * pageSize)
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
            catch (Exception ex)
            {
                await _logRepository.AddLog(new LogModel
                {
                    code = LogEventCode.DB_SAVE_ERROR,
                    message = "Erro ao carregar históricos com includes",
                    source = "HistoryRepository",
                    details = ex.ToString(),
                    remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(_httpContextAccessor.HttpContext)
                });

                return new PaginatedResult<HistoryModel>();
            }
        }
    }
}
