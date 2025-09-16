using HeimdallWeb.Data;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;
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

        public async Task<PaginatedResult<UserModel>?> getUsers(string? where, int page, int pageSize)
        {
            try
            {
                var query = _appDbContext.User.AsQueryable();

                if (!string.IsNullOrEmpty(where))
                {
                    query = query
                        .Where(u => u.username.Contains(where) ||
                               u.email.Contains(where)); 
                }

                var totalCount = await query.CountAsync();

                var items = await query
                 .OrderBy(u => u.user_id)
                 .Skip((page - 1) * pageSize)
                 .Take(pageSize)
                 .ToListAsync();

                return new PaginatedResult<UserModel>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception)
            {
                return new PaginatedResult<UserModel>();
            }
        }

        public async Task<UserModel?> getUserById(int id)
        {
            return await _appDbContext.User.FirstOrDefaultAsync(x => x.user_id == id);
        }

        public async Task<UserModel> insertUser(UserModel user)
        {
            user.password = user.hashUserPassword();  
            user.created_at = DateTime.Now;
            user.username = user.username.Trim();
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
            return await _appDbContext.User.AsNoTracking().FirstOrDefaultAsync(x => x.email == emailOrUsername || x.username == emailOrUsername); 
        }

        public async Task<bool> verifyIfUserExists(UserModel user)
        {
            return await _appDbContext.User.AsNoTracking().
                AnyAsync(x => x.username == user.username || x.email == user.email); 
        }

        public async Task<bool> verifyIfUserExistsWithLogin(UserModel user)
        {
            return await _appDbContext.User.AsNoTracking().AnyAsync(x => x.username == user.username);
        }

        public async Task<bool> verifyIfUserExistsWithEmail(UserModel user)
        {
            return await _appDbContext.User.AsNoTracking().AnyAsync(x => x.email == user.email);
        }
    }
}
