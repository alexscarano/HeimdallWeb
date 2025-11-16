using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HeimdallWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserRepository userRepository,
            IDashboardRepository dashboardRepository,
            ILogger<AdminController> logger)
        {
            _userRepository = userRepository;
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Dashboard administrativo com estatísticas agregadas.
        /// Rota: /admin/dashboard
        /// </summary>
        [Authorize(Roles = "2")]
        [HttpGet]
        public async Task<IActionResult> Dashboard(
            int logPage = 1, 
            int logPageSize = 10,
            string? logLevel = null,
            DateTime? logStartDate = null,
            DateTime? logEndDate = null)
        {
            try
            {
                // Garantir valores mínimos
                logPage = Math.Max(logPage, 1);
                logPageSize = Math.Min(Math.Max(logPageSize, 5), 50); // entre 5 e 50
                
                // Passar filtros para ViewData para repopular form
                ViewData["LogLevel"] = logLevel;
                ViewData["LogStartDate"] = logStartDate?.ToString("yyyy-MM-dd");
                ViewData["LogEndDate"] = logEndDate?.ToString("yyyy-MM-dd");
                
                var viewModel = await _dashboardRepository.GetAdminDashboardDataAsync(
                    logPage, logPageSize, logLevel, logStartDate, logEndDate);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard administrativo");
                TempData["ErrorMsg"] = "Erro ao carregar o dashboard. Tente novamente.";
                return View(new AdminDashboardViewModel());
            }
        }

        [Authorize(Roles = "2")]
        public async Task<IActionResult> GerenciarUsuarios(
            string? where, 
            int page = 1, 
            int pageSize = 10,
            bool? isActive = null,
            bool? isAdmin = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null)
        {
            int maxPageSize = 10;
            pageSize = Math.Min(pageSize, maxPageSize);
            page = Math.Max(page, 1);

            ViewData["CurrentSearch"] = where;
            ViewData["IsActive"] = isActive;
            ViewData["IsAdmin"] = isAdmin;
            ViewData["CreatedFrom"] = createdFrom?.ToString("yyyy-MM-dd");
            ViewData["CreatedTo"] = createdTo?.ToString("yyyy-MM-dd");

            var users = await _userRepository.getUsers(where, page, pageSize, isActive, isAdmin, createdFrom, createdTo) 
                ?? throw new Exception("Ocorreu um erro ao consultar os usuários");

            return View(users);
        }

        [Authorize(Roles = "2")]
        [HttpPost]
        public async Task<JObject> ToggleUserStatus(int id, bool isActive)
        {
            try
            {
                var userDB = await _userRepository.getUserById(id);
                if (userDB is null)
                    return JObject.FromObject(new { success = false, message = "Usuário não encontrado." });

                if (userDB.user_type == 2) // Admin
                    return JObject.FromObject(new { success = false, message = "Não é possível bloquear administradores." });

                bool toggled = await _userRepository.ToggleUserActiveStatus(id, isActive);
                if (toggled)
                {
                    string action = isActive ? "desbloqueado" : "bloqueado";
                    return JObject.FromObject(new { success = true, message = $"Usuário {action} com sucesso." });
                }

                return JObject.FromObject(new { success = false, message = "Falha ao alterar status do usuário." });
            }
            catch (Exception ex)
            {
                return JObject.FromObject(new { success = false, message = "Erro: " + ex.Message });
            }
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

                    bool deleted = await _userRepository.DeleteUser(id);
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
