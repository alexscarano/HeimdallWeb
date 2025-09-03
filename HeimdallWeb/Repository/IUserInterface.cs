using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public interface IUserInterface
    {
        List<UserModel> getAllUsers();

        UserModel insertUser(UserModel user);

        UserModel getUserById(int id);

        UserModel updateUser(UserModel user);

        bool deleteUser(int id);
    }
}
