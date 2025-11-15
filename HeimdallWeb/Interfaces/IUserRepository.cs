using HeimdallWeb.DTO;
using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;

namespace HeimdallWeb.Interfaces
{
    public interface IUserRepository
    {
        Task<PaginatedResult<UserModel>?> getUsers(string ?where, int page, int pageSize);

        Task<PaginatedResult<UserModel>?> getUsers(string? where, int page, int pageSize, bool? isActive, bool? isAdmin, DateTime? createdFrom, DateTime? createdTo);

        Task<UserModel> InsertUser(UserModel user);

        Task<UserModel?> getUserById(int id);

        Task<UpdateUserDTO> UpdateUser(UpdateUserDTO user);

        Task<bool> DeleteUser(int id);

        Task<bool> ToggleUserActiveStatus(int id, bool isActive);

        Task<UserModel?> GetUserByEmailOrLogin(string emailOrUsername);

        Task<bool> VerifyIfUserExists(UserModel user);

        Task<bool> VerifyIfUserExistsWithLogin(UpdateUserDTO user);

        Task<bool> VerifyIfUserExistsWithEmail(UpdateUserDTO user);

        Task<bool> VerifyIfUserExistsWithLogin(UserModel user);

        Task<bool> VerifyIfUserExistsWithEmail(UserModel user);
    }
}
