using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public interface IUserRepository
    {
        List<UserModel> getAllUsers();

        UserModel insertUser(UserModel user);

        UserModel getUserById(int id);

        UserModel updateUser(UserModel user);

        bool deleteUser(int id);

        UserModel? getUserByEmailOrLogin(string emailOrUsername);

        public bool verifyIfUserExists(UserModel user);
    }
}
