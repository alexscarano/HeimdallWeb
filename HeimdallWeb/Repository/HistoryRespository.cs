using HeimdallWeb.Data;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;

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

        public Task<bool> deleteHistory(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<HistoryModel>> getAllHistories()
        {
            throw new NotImplementedException();
        }

        public Task<HistoryModel?> getHistoryById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<HistoryModel> insertHistory(HistoryModel history)
        {
            int user_id = CookiesHelper.getUserIDFromCookie(CookiesHelper.getAuthCookie(_httpContextAccessor.HttpContext.Request));

            history.created_date = DateTime.UtcNow;
            history.user_id = user_id;
            await _appDbContext.History.AddAsync(history);
            await _appDbContext.SaveChangesAsync();

            return history; 
        }
    }
}
