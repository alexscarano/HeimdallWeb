using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;
using HeimdallWeb.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.Controllers;

public class LoginController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    public LoginController(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Enter(LoginDTO user)
    {
        try
        {
            if (ModelState.IsValid)
            {
                UserModel userDB = await _userRepository.getUserByEmailOrLogin(user.emailOrLogin) ?? throw new Exception("Não foi possivel consultar");
               
                if (PasswordUtils.VerifyPassword(user.password, userDB.password))
                {
                    string token = TokenService.generateToken(userDB, _config);

                    if (!string.IsNullOrEmpty(token))
                    {
                        TempData["OkMsg"] = "Login concluido com sucesso!";
                        CookiesHelper.generateAuthCookie(Response, token);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            TempData["ErrorMsg"] = "Credenciais inválidas";
        }
        catch (System.Exception)
        {
            TempData["ErrorMsg"] = "Credenciais inválidas";
        }
        return View("Index");
    }

    public IActionResult Logout()
    {
        try
        {
            var cookieString = CookiesHelper.getAuthCookie(Request);

            if (!string.IsNullOrEmpty(cookieString))
            {
                CookiesHelper.deleteAuthCookie(Response);
                TempData["OkMsg"] = "Logout concluido com sucesso, volte em breve!";
            }
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Ocorreu um erro ao fazer o logout";
        }
        return RedirectToAction("Index", "Home");
    }
}