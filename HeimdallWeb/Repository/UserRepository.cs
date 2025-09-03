using HeimdallWeb.Data;
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
            user.password = user.hashUserPassword();  
            user.created_at = DateTime.Now;
            _appDbContext.User.Add(user);
            _appDbContext.SaveChanges();

            return user;
        }

        public UserModel updateUser(UserModel user)
        {
            UserModel userDB = getUserById(user.user_id);

            if (userDB == null) throw new Exception("Houve um erro ao tentar atualizar o usuário")

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

            if (userDB == null) throw new Exception("Houve um erro ao deletar o usuário");

            _appDbContext.User.Remove(userDB);
            _appDbContext.SaveChanges();

            return true;
        }

    }
}
