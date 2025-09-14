using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;
using HeimdallWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.Controllers;

public class UserController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    public UserController(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    [Authorize]
    public IActionResult Index()
    {
        return View();  
    }


    [Authorize]
    public IActionResult Delete()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> History()
    {
        var users = await _userRepository.getAllUsers() ?? throw new Exception("Ocorreu um erro ao consultar os usuários");

        return View(users);
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [Authorize]
    public IActionResult Edit()
    {
        return View();
    }    

    [HttpPost]
    public async Task<IActionResult> RegisterAction(UserModel user)
    {
        try
        {
            if (ModelState.IsValid)
            {

                if (!await _userRepository.verifyIfUserExists(user))
                {
                    await _userRepository.insertUser(user);
                    string token = TokenService.generateToken(user, _config);
                    CookiesHelper.generateAuthCookie(Response, token);  
                    TempData["OkMsg"] = "O usuário foi cadastrado com sucesso";
                    return RedirectToAction("Index", "Home");
                }
                TempData["ErrorMsg"] = "Este usuário já está cadastrado, verique o seu email/nome de usuário";
            }
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Ocorreu um erro no cadastro do usuário";
        }
        return View("Register", user);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EditAction(UserModel model)
    {
        try
        {
            ModelState.Remove(nameof(model.user_id));

            if (!ModelState.IsValid)
                return View("Edit", model);

            // Pega o user_id dos claims validados pelo middleware (preferível)
            int? userId = TokenService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return View("Edit", model);
            }

            model.user_id = userId.Value;

            if (await _userRepository.verifyIfUserExistsWithLogin(model))
            {
                TempData["ErrorMsg"] = "Já existe um usuário com este login, tente outro.";
                return View("Edit", model);
            }

            if (await _userRepository.verifyIfUserExistsWithEmail(model))
            {
                TempData["ErrorMsg"] = "Já existe um usuário com este email, tente outro.";
                return View("Edit", model);
            }

            // buscar usuário e atualizar campos permitidos
            var userDb = await _userRepository.getUserById(model.user_id);
            if (userDb == null)
            {
                TempData["ErrorMsg"] = "Ocorreu um erro ao atualizar o usuário.";
                return View("Edit", model);
            }

            await _userRepository.updateUser(model);

            TempData["OkMsg"] = "Usuário atualizado com sucesso";
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Ocorreu um erro ao atualizar o usuário.";
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteAction(int id, DeleteUserDTO userToDelete)
    {
        try
        {
            int? userId = TokenService.GetUserIdFromClaims(User);
            if (userId == null)
            {
                return View("Edit", userToDelete);
            }

            id = userId.Value;

            var userDB = await _userRepository.getUserById(id) ?? throw new Exception("Náo foi possável fazer a consulta do usuário");

            if (PasswordUtils.VerifyPassword(userToDelete.password, userDB.password))
            {
                bool deleted = await _userRepository.deleteUser(id);

                if (deleted)
                {
                    CookiesHelper.deleteAuthCookie(Response);
                    TempData["OkMsg"] = "Usuário deletado com sucesso, volte sempre.";
                    return RedirectToAction("Index", "Login");
                }
            }
            TempData["ErrorMsg"] = "Senha incorreta, tente novamente.";
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Ocorreu um erro ao tentar deletar o usuário, tente novamente.";
            return View("Delete");
        }

        return View("Delete", userToDelete);
    }

}