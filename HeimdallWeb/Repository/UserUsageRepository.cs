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
            await _dbContext.SaveChangesAsync();
            return true; 
        }
        catch 
        {
            return false;
        }
    }


    public async Task<(int count, UserUsageModel obj)> GetUserUsageCount(int userId, DateTime date)
    {
        try 
        {
            var count = await _dbContext.UserUsage
                .Where(u => u.user_id == userId && u.date.Date == date.Date)
                .CountAsync();

                var userUsage = await _dbContext.UserUsage
                .OrderByDescending(u => u.date)
                .FirstOrDefaultAsync(u => u.user_id == userId);

            return (count, userUsage);
        }
        catch
        {
            return (-1, new UserUsageModel());
        }
    }

}
