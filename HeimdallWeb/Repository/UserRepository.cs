using HeimdallWeb.Data;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;

        public UserRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<UserModel>?> getAllUsers()
        {
            List<UserModel> users;
            try
            {
              users = await _appDbContext.User.ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }

            return users;
        }

        public async Task<UserModel?> getUserById(int id)
        {
            return await _appDbContext.User.FirstOrDefaultAsync(x => x.user_id == id);
        }

        public async Task<UserModel> insertUser(UserModel user)
        {
            user.password = user.hashUserPassword();  
            user.created_at = DateTime.Now;
            await _appDbContext.User.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            return user;
        }

        public async Task<UserModel> updateUser(UserModel user)
        {
            UserModel userDB = await getUserById(user.user_id) ?? throw new Exception("Houve um erro ao atualizar o usuário");

            if (userDB.user_id != user.user_id) throw new Exception("Houve um erro ao atualizar o usuário");

            userDB.username = user.username;
            userDB.password = user.password.hashPassword();
            userDB.email = user.email;  
            userDB.user_type = user.user_type;
            userDB.updated_at = DateTime.Now;

            await _appDbContext.SaveChangesAsync();

            return user;
        }

        public async Task<bool> deleteUser(int id)
        {
            UserModel userDB = await getUserById(id) ?? throw new Exception("Houve um erro ao deletar o usuário");

            _appDbContext.User.Remove(userDB);
            _appDbContext.SaveChanges();

            return true;
        }

        public async Task<UserModel?> getUserByEmailOrLogin(string emailOrUsername)
        {
            return await _appDbContext.User.FirstOrDefaultAsync(x => x.email == emailOrUsername || x.username == emailOrUsername); 
        }

        public async Task<bool> verifyIfUserExists(UserModel user)
        {
            return await _appDbContext.User.
                AnyAsync(x => x.username == user.username || x.email == user.email); 
        }

        public async Task<bool> verifyIfUserExistsWithLogin(UserModel user)
        {
            return await _appDbContext.User.AnyAsync(x => x.username == user.username);
        }

        public async Task<bool> verifyIfUserExistsWithEmail(UserModel user)
        {
            return await _appDbContext.User.AnyAsync(x => x.email == user.email);
        }
    }
}
