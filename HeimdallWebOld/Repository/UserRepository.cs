using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Enums;
using HeimdallWeb.Models.Map;

namespace HeimdallWeb.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogRepository _logRepository;

        public UserRepository(AppDbContext appDbContext, ILogRepository logRepository)
        {
            _appDbContext = appDbContext;
            _logRepository = logRepository;
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

        public async Task<PaginatedResult<UserModel>?> getUsers(string? where, int page, int pageSize, bool? isActive, bool? isAdmin, DateTime? createdFrom, DateTime? createdTo)
        {
            try
            {
                var query = _appDbContext.User.AsQueryable();

                // Filtro de busca textual
                if (!string.IsNullOrEmpty(where))
                {
                    query = query.Where(u => u.username.Contains(where) || u.email.Contains(where));
                }

                // Filtro de status ativo/bloqueado
                if (isActive.HasValue)
                {
                    query = query.Where(u => u.is_active == isActive.Value);
                }

                // Filtro de tipo de usuário (admin ou não)
                if (isAdmin.HasValue)
                {
                    int userType = isAdmin.Value ? 2 : 1; // 2 = Admin, 1 = Default
                    query = query.Where(u => u.user_type == userType);
                }

                // Filtro de data de criação (de)
                if (createdFrom.HasValue)
                {
                    query = query.Where(u => u.created_at >= createdFrom.Value);
                }

                // Filtro de data de criação (até)
                if (createdTo.HasValue)
                {
                    var endOfDay = createdTo.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(u => u.created_at <= endOfDay);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(u => u.created_at)
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

        public async Task<bool> ToggleUserActiveStatus(int id, bool isActive)
        {
            try
            {
                var user = await getUserById(id);
                if (user is null) return false;

                user.is_active = isActive;
                user.updated_at = DateTime.Now;
                
                await _appDbContext.SaveChangesAsync();

                await _logRepository.AddLog(new LogModel
                {
                    code = LogEventCode.DB_SAVE_OK,
                    message = $"Usuário {(isActive ? "desbloqueado" : "bloqueado")} com sucesso",
                    source = "UserRepository",
                    user_id = user.user_id,
                    details = $"Usuário: {user.username}, Status: {(isActive ? "Ativo" : "Bloqueado")}",
                    remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(null)
                });

                return true;
            }
            catch (Exception ex)
            {
                await _logRepository.AddLog(new LogModel
                {
                    code = LogEventCode.DB_SAVE_ERROR,
                    message = "Erro ao alterar status do usuário",
                    source = "UserRepository",
                    details = ex.ToString(),
                    remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(null)
                });
                return false;
            }
        }

        public async Task<UserModel?> getUserById(int id)
        {
            return await _appDbContext.User.FirstOrDefaultAsync(x => x.user_id == id);
        }

        public async Task<UserModel> InsertUser(UserModel user)
        {
            user.password = user.hashUserPassword();  
            user.username = user.username.Trim().ToLower();
            await _appDbContext.User.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.DB_SAVE_OK,
                message = "Registro salvo com sucesso",
                source = "UserRepository",
                user_id = user.user_id,
                details = $"Novo usuário criado: {user.username}",
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(null)
            });

            return user;
        }

        public async Task<UpdateUserDTO> UpdateUser(UpdateUserDTO user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            bool hasUsername = !string.IsNullOrWhiteSpace(user.username);
            bool hasEmail = !string.IsNullOrWhiteSpace(user.email);
            bool hasPassword = !string.IsNullOrWhiteSpace(user.password);
            bool hasImage = !string.IsNullOrEmpty(user.profile_image_path);

            if (!hasUsername && !hasEmail && !hasPassword && !hasImage)
                throw new ArgumentException("Nenhum campo para atualizar.");

            if (hasPassword)
            {
                if (string.IsNullOrWhiteSpace(user.confirm_password) || user.password != user.confirm_password)
                    throw new ArgumentException("As senhas precisam coincidir.");
            }

            UserModel userDB = await getUserById(user.user_id) ?? throw new Exception("Houve um erro ao atualizar o usuário");

            if (userDB.user_id != user.user_id) throw new Exception("Houve um erro ao atualizar o usuário");

            if (hasUsername)
                userDB.username = user.username!.Trim().ToLowerInvariant();

            if (hasPassword)
                userDB.password = user.password!.hashPassword();

            if (hasEmail)
                userDB.email = user.email!.Trim().ToLowerInvariant();

            if (hasImage)
                userDB.profile_image = user.profile_image_path;

            userDB.updated_at = DateTime.Now;

            await _appDbContext.SaveChangesAsync();
            
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.DB_SAVE_OK,
                message = "Registro salvo com sucesso",
                source = "UserRepository",
                user_id = userDB.user_id,
                details = $"Usuário atualizado: {userDB.username}",
                remote_ip = NetworkUtils.GetRemoteIPv4OrFallback(null)
            });

            return user;
        }

        public async Task<bool> DeleteUser(int id)
        {
            UserModel userDB = await getUserById(id) ?? throw new Exception("Houve um erro ao deletar o usuário");

            _appDbContext.User.Remove(userDB);
            _appDbContext.SaveChanges();

            return true;
        }

        public async Task<UserModel?> GetUserByEmailOrLogin(string emailOrUsername)
        {
            return await _appDbContext.User.AsNoTracking().FirstOrDefaultAsync(x => x.email == emailOrUsername || x.username == emailOrUsername); 
        }

        public async Task<bool> VerifyIfUserExists(UserModel user)
        {
            return await _appDbContext.User.AsNoTracking().
                AnyAsync(x => x.username == user.username || x.email == user.email); 
        }

        public async Task<bool> VerifyIfUserExistsWithLogin(UserModel user)
        {
            return await _appDbContext.User.AsNoTracking().AnyAsync(x => x.username == user.username);
        }

        public async Task<bool> VerifyIfUserExistsWithLogin(UpdateUserDTO user)
        {
            return await _appDbContext.User.AsNoTracking().AnyAsync(x => x.username == user.username);
        }

        public async Task<bool> VerifyIfUserExistsWithEmail(UserModel user)
        {
            return await _appDbContext.User.AsNoTracking().AnyAsync(x => x.email == user.email);
        }

        public async Task<bool> VerifyIfUserExistsWithEmail(UpdateUserDTO user)
        {
            return await _appDbContext.User.AsNoTracking().AnyAsync(x => x.email == user.email);
        }
    }
}
