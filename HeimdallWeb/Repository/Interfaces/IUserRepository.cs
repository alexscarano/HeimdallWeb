using HeimdallWeb.DTO;
using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;

namespace HeimdallWeb.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<PaginatedResult<UserModel>?> getUsers(string ?where, int page, int pageSize);

        Task<UserModel> insertUser(UserModel user);

        Task<UserModel?> getUserById(int id);

        Task<UpdateUserDTO> updateUser(UpdateUserDTO user);

        Task<bool> deleteUser(int id);

        Task<UserModel?> getUserByEmailOrLogin(string emailOrUsername);

        Task<bool> verifyIfUserExists(UserModel user);

        Task<bool> verifyIfUserExistsWithLogin(UpdateUserDTO user);

        Task<bool> verifyIfUserExistsWithEmail(UpdateUserDTO user);

        Task<bool> verifyIfUserExistsWithLogin(UserModel user);

        Task<bool> verifyIfUserExistsWithEmail(UserModel user);
    }
}
