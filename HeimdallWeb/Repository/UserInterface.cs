using HeimdallWeb.Data;
using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public class UserInterface : IUserInterface
    {
        private readonly AppDbContext _appDbContext;

        public UserInterface(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public List<UserModel> getAllUsers()
        {
            return _appDbContext.User.ToList();
        }

        public UserModel getUserById(int id)
        {
            return _appDbContext.User.FirstOrDefault(x => x.user_id == id);
        }

        public UserModel insertUser(UserModel user)
        {
            user.hashUserPassword();  
            user.created_at = DateTime.Now;
            _appDbContext.User.Add(user);
            _appDbContext.SaveChanges();

            return user;
        }

        public UserModel updateUser(UserModel user)
        {
            UserModel userDB = getUserById(user.user_id);

            userDB.username = user.username;
            userDB.password = user.password;
            userDB.email = user.email;  
            userDB.user_type = user.user_type;
            userDB.updated_at = DateTime.Now;

            _appDbContext.User.Add(user);
            _appDbContext.SaveChanges();

            return user;
        }

        public bool deleteUser(int id)
        {
            UserModel userDB = getUserById(id);

            if (userDB == null) throw new Exception("Error on deleting the user");

            _appDbContext.User.Remove(userDB);
            _appDbContext.SaveChanges();

            return true;
        }

    }
}
