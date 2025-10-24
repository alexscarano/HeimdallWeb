using HeimdallWeb.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Controllers
{
    public class AdminController : Controller
    {
        private IUserRepository _userRepository;
        public AdminController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "2")]
        public async Task<IActionResult> DashboardAdmin(string? where, int page = 1, int pageSize = 10)
        {
            int maxPageSize = 10;
            pageSize = Math.Min(pageSize, maxPageSize);
            page = Math.Max(page, 1);

            ViewData["CurrentSearch"] = where;

            var users = await _userRepository.getUsers(where, page, pageSize) ?? throw new Exception("Ocorreu um erro ao consultar os usuários");

            return View(users);
        }


        [Authorize(Roles = "2")]
        [HttpPost]
        public async Task<JObject> DeleteUser(int id)
            {
                try
                {
                    var userDB = await _userRepository.getUserById(id);
                    if (userDB is null)
                        return JObject.FromObject(new { success = false, message = "Usuário não encontrado." });

                    bool deleted = await _userRepository.deleteUser(id);
                    if (deleted)
                        return JObject.FromObject(new { success = true, message = "Usuário deletado com sucesso." });

                    return JObject.FromObject(new { success = false, message = "Falha ao deletar usuário." });
                }
                catch (Exception ex)
                {
                    return JObject.FromObject(new { success = false, message = "Erro: " + ex.Message });
                }
            }
    }
}
