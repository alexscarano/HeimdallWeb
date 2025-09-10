using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public interface IUserRepository
    {
        Task<List<UserModel>?> getAllUsers();

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
