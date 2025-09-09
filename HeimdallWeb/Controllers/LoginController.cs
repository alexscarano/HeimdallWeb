using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;
using HeimdallWeb.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.Controllers;

public class LoginController : Controller
{
    private readonly IUserRepository _userRepository;

    public LoginController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IActionResult Index()
    {

        return View();
    }

    [HttpPost]
    public IActionResult Enter(LoginDTO user)
    {
        try
        {
            if (ModelState.IsValid)
            {
                UserModel userDB = _userRepository.getUserByEmailOrLogin(user.emailOrLogin) ?? throw new Exception("Não foi possivel consultar");

                if (PasswordUtils.VerifyPassword(user.password, userDB.password))
                {
                    TempData["OkMsg"] = "Login concluido com sucesso!";
                    return RedirectToAction("Index", "Home");
                }

            }
            TempData["ErrorMsg"] = "Creedenciais inválidas";
        }
        catch (System.Exception)
        {
            TempData["ErrorMsg"] = "Creedenciais inválidas";
        }
        return View("Index");
    }
}