using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.Controllers;

public class LoginController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly ILogRepository _logRepository;
    public LoginController(IUserRepository userRepository, IConfiguration config, ILogRepository logRepository)
    {
        _userRepository = userRepository;
        _config = config;
        _logRepository = logRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
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
                UserModel userDB = await _userRepository.GetUserByEmailOrLogin(user.emailOrLogin.ToLower()) ?? throw new Exception("Não foi possivel consultar");
               
                if (PasswordUtils.VerifyPassword(user.password, userDB.password))
                {
                    string token = TokenService.generateToken(userDB, _config);

                    if (!string.IsNullOrEmpty(token))
                    {
                        await _logRepository.AddLog(new LogModel
                        {
                            code = LogEventCode.USER_LOGIN,
                            message = "Usuário autenticado com sucesso",
                            source = "LoginController",
                            user_id = userDB.user_id,
                            remote_ip = HttpContext.Connection.RemoteIpAddress?.ToString()
                        });
                        TempData["OkMsg"] = "Login concluido com sucesso!";
                        CookiesHelper.generateAuthCookie(Response, token);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.USER_LOGIN_FAILED,
                message = "Falha na autenticação do usuário",
                source = "LoginController",
                details = $"Tentativa de login com: {user.emailOrLogin}",
                remote_ip = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            TempData["ErrorMsg"] = "Credenciais inválidas";
            return RedirectToAction("Index", "Home");
        }
        catch (System.Exception ex)
        {
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.USER_LOGIN_FAILED,
                message = "Falha na autenticação do usuário",
                source = "LoginController",
                details = ex.Message,
                remote_ip = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            TempData["ErrorMsg"] = "Credenciais inválidas";
        }
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
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