using HeimdallWeb.Models;

namespace HeimdallWeb.Interfaces;

public interface IUserUsageRepository
{
    public Task<bool> AddUserUsage(UserUsageModel userUsage);

    public Task<(int count, UserUsageModel obj)> GetUserUsageCount(int userId, DateTime date);

}
