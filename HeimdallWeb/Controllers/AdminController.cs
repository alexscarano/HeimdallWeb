using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> DashboardAdmin()
        {
            var users = await _userRepository.getAllUsers() ?? throw new Exception("Ocorreu um erro ao consultar os usuários");

            return View(users);
        }


        //[Authorize(Roles = "2")]
        //[HttpPost]
        //public async Task<JsonResult> DeleteUserAsync(int id)
        //{
        //    try
        //    {
        //        var userDB = await _userRepository.getUserById(id);
        //        if (userDB == null)
        //            return Json(new { success = false, message = "Usuário não encontrado." });

        //        bool deleted = await _userRepository.deleteUser(id);
        //        if (deleted)
        //            return Json(new { success = true, message = "Usuário deletado com sucesso." });

        //        return Json(new { success = false, message = "Falha ao deletar usuário." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Erro: " + ex.Message });
        //    }
        //}

    }
}
