using HeimdallWeb.Models;

namespace HeimdallWeb.Helpers
{
    public interface ISession
    {
        void CreateUserSession(UserModel user);
        void DeleteUserSession();
        UserModel GetUserModel();
    }
}
