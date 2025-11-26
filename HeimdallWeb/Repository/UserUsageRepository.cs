using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Repository;

public class UserUsageRepository : IUserUsageRepository
{
    private readonly AppDbContext _dbContext;

    public UserUsageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddUserUsage(UserUsageModel userUsage)
    {
        try
        {
            if (userUsage is null)
                return false;

            _dbContext.UserUsage.Add(userUsage);
            // SaveChangesAsync será chamado no ScanService dentro da transação
            return true; 
        }
        catch 
        {
            return false;
        }
    }


    public async Task<(int count, UserUsageModel obj, bool isUserAdmin)> GetUserUsageCount(int userId, DateTime date)
    {
        try 
        {
            var count = await _dbContext.UserUsage
                .Where(u => u.user_id == userId && u.date.Date == date.Date)
                .CountAsync();

                var userUsage = await _dbContext.UserUsage
                .OrderByDescending(u => u.date)
                .FirstOrDefaultAsync(u => u.user_id == userId);

                var isUserAdmin = await _dbContext.User
                    .AnyAsync(u => u.user_id == userId && u.user_type == 2);

            return (count, userUsage is not null ? userUsage : new UserUsageModel(), isUserAdmin);
        }
        catch
        {
            return (-1, new UserUsageModel(), false);
        }
    }

}
