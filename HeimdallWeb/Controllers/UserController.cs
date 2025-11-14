using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
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
    public IActionResult Delete()
    {
        return View();
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
    public async Task<IActionResult> Register(Models.UserModel user)
    {
        try
        {
            ModelState.Remove(nameof(user.Histories));
            if (ModelState.IsValid)
            {

                if (!await _userRepository.VerifyIfUserExists(user))
                {
                    await _userRepository.InsertUser(user);
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
    public async Task<IActionResult> Edit(UpdateUserDTO model)
    {
        try
        {
            ModelState.Remove(nameof(model.user_id));
            if (!ModelState.IsValid)
            {
                TempData["ErrorMsg"] = "Ocorreu um erro ao atualizar o usuário";
                return View("Edit", model);
            }

            int? userId = TokenService.GetUserIdFromClaims(User);
            // Pega o user_id dos claims validados pelo middleware (preferível)
            if (userId is null)
            {
                TempData["ErrorMsg"] = "Ocorreu um erro ao atualizar o usuário";
                return View("Edit", model);
            }

            model.user_id = userId.Value;

            if (await _userRepository.VerifyIfUserExistsWithLogin(model))
            {
                TempData["ErrorMsg"] = "Já existe um usuário com este login, tente outro.";
                return View("Edit", model);
            }

            if (await _userRepository.VerifyIfUserExistsWithEmail(model))
            {
                TempData["ErrorMsg"] = "Já existe um usuário com este email, tente outro.";
                return View("Edit", model);
            }

            // buscar usuário e atualiza campos permitidos
            var userDb = await _userRepository.getUserById(userId.Value);
            if (userDb == null)
            {
                TempData["ErrorMsg"] = "Ocorreu um erro ao atualizar o usuário.";
                return View("Edit", model);
            }

            await _userRepository.UpdateUser(model);

            TempData["OkMsg"] = "Usuário atualizado com sucesso";
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Ocorreu um erro ao atualizar o usuário.";
            return RedirectToAction("Edit", model);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Delete(int id, DeleteUserDTO userToDelete)
    {
        try
        {
            int? userId = TokenService.GetUserIdFromClaims(User);
            if (userId is null)
            {
                return View("Edit", userToDelete);
            }

            id = userId.Value;

            var userDB = await _userRepository.getUserById(id) ?? throw new Exception("Náo foi possável fazer a consulta do usuário");

            if (PasswordUtils.VerifyPassword(userToDelete.password, userDB.password))
            {
                bool deleted = await _userRepository.DeleteUser(id);

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