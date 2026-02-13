using HeimdallWeb.Models;

namespace HeimdallWeb.Interfaces;

public interface IUserUsageRepository
{
    public Task<bool> AddUserUsage(UserUsageModel userUsage);

    public Task<(int count, UserUsageModel obj, bool isUserAdmin)> GetUserUsageCount(int userId, DateTime date);

}
