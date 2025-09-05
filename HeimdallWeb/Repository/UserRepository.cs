using HeimdallWeb.Data;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;

        public UserRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public List<UserModel>? getAllUsers()
        {
            List<UserModel> users;
            try
            {
              users = _appDbContext.User.ToList();
            }
            catch (Exception)
            {
                return null;
            }

            return users;
        }

        public UserModel? getUserById(int id)
        {
            return _appDbContext.User.FirstOrDefault(x => x.user_id == id);
        }

        public UserModel insertUser(UserModel user)
        {
            user.password = user.hashUserPassword();  
            user.created_at = DateTime.Now;
            _appDbContext.User.Add(user);
            _appDbContext.SaveChanges();

            return user;
        }

        public UserModel updateUser(UserModel user)
        {
            UserModel userDB = getUserById(user.user_id) ?? throw new Exception("Houve um erro ao atualizar o usuário");

            if (userDB.user_id != user.user_id) throw new Exception("Houve um erro ao atualizar o usuário");

            userDB.username = user.username;
            userDB.password = user.password.hashPassword();
            userDB.email = user.email;  
            userDB.user_type = user.user_type;
            userDB.updated_at = DateTime.Now;

            _appDbContext.SaveChanges();

            return user;
        }

        public bool deleteUser(int id)
        {
            UserModel userDB = getUserById(id) ?? throw new Exception("Houve um erro ao deletar o usuário");

            _appDbContext.User.Remove(userDB);
            _appDbContext.SaveChanges();

            return true;
        }

        public UserModel? getUserByEmailOrLogin(string emailOrUsername)
        {
            return _appDbContext.User.FirstOrDefault(x => x.email == emailOrUsername || x.username == emailOrUsername); 
        }

        public bool verifyIfUserExists(UserModel user)
        {
            return _appDbContext.User.
                Any(x => x.username == user.username || x.email == user.email); 
        }
    }
}
