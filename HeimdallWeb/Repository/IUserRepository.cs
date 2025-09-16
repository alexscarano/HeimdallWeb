using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;

namespace HeimdallWeb.Repository
{
    public interface IUserRepository
    {
        Task<PaginatedResult<UserModel>?> getUsers(string ?where, int page, int pageSize);

        Task<UserModel> insertUser(UserModel user);

        Task<UserModel?> getUserById(int id);

        Task<UserModel> updateUser(UserModel user);

        Task<bool> deleteUser(int id);

        Task<UserModel?> getUserByEmailOrLogin(string emailOrUsername);

        Task<bool> verifyIfUserExists(UserModel user);

        Task<bool> verifyIfUserExistsWithLogin(UserModel user);

        Task<bool> verifyIfUserExistsWithEmail(UserModel user);
    }
}
