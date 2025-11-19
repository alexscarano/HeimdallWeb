using System.Runtime.CompilerServices;
using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Services;
using HeimdallWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.Controllers;

public class UserController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly IUserStatisticsRepository _userStatisticsRepository;

    public UserController(
        IUserRepository userRepository, 
        IConfiguration config, 
        IUserStatisticsRepository userStatisticsRepository
    )
    {
        _userRepository = userRepository;
        _config = config;
        _userStatisticsRepository = userStatisticsRepository;
    }

    public IActionResult Login() => View();
    public IActionResult Register() => View();


    [Authorize]
    public async Task<IActionResult> Profile()
    {
        int? userId = TokenService.GetUserIdFromClaims(User);
        if (userId is null) return RedirectToAction("Index", "Home");

        var user = await _userRepository.getUserById(userId.Value);
        if (user == null) return NotFound();

        var model = new UserEditViewModel
        {
            UpdateUser = new UpdateUserDTO
            {
                user_id = user.user_id,
                username = user.username,
                email = user.email,
                profile_image_path = user.profile_image ?? "~/uploads/users/default_user_image.png",
            },
            DeleteUser = new DeleteUserDTO()
        };

        // Pass the model to the view so the form includes the current user_id and other values
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Statistics()
    {
        int? userId = TokenService.GetUserIdFromClaims(User);
        if (userId is null)
        {
            TempData["ErrorMsg"] = "Usuário não autenticado.";
            return RedirectToAction("Index", "Home");
        }

        var statistics = await _userStatisticsRepository.GetUserStatisticsAsync(userId.Value);
        return View(statistics);
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
    public async Task<IActionResult> Profile(UserEditViewModel model, string? action)
    {
        try
        {
            // Remove model state entries for the unrelated nested model depending on the action
            if (action == "update")
            {
                var keysToRemove = ModelState.Keys.Where(k => k.StartsWith(nameof(model.DeleteUser))).ToList();
                ModelState.Remove($"{nameof(model.UpdateUser)}.{nameof(model.UpdateUser.user_id)}");
                foreach (var k in keysToRemove) ModelState.Remove(k);
            }
            else if (action == "delete")
            {
                var keysToRemove = ModelState.Keys.Where(k => k.StartsWith(nameof(model.UpdateUser))).ToList();
                foreach (var k in keysToRemove) ModelState.Remove(k);
            }

            int? userId = TokenService.GetUserIdFromClaims(User);
            if (userId is null)
            {
                TempData["ErrorMsg"] = "Ocorreu um erro ao atualizar o usuário";
                return View("Profile", model);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMsg"] = "Ocorreu um erro ao atualizar o usuário";
                return View("Profile", model);
            }

            if (action == "update")
            {
                // garante que o user_id é o do token
                model.UpdateUser.user_id = userId.Value;

                if (await _userRepository.VerifyIfUserExistsWithLogin(model.UpdateUser))
                {
                    TempData["ErrorMsg"] = "Já existe um usuário com este login, tente outro.";
                    return RedirectToAction("Profile", "User");
                }

                if (await _userRepository.VerifyIfUserExistsWithEmail(model.UpdateUser))
                {
                    TempData["ErrorMsg"] = "Já existe um usuário com este email, tente outro.";
                    return RedirectToAction("Profile", "User");
                }

                var userDb = await _userRepository.getUserById(userId.Value);
                if (userDb is null)
                {
                    TempData["ErrorMsg"] = "Usuário não encontrado.";
                    return RedirectToAction("Profile", "User");
                }

                if (model.ProfileImage is not null)
                {
                    if (!model.ProfileImage.IsImageFile())
                        throw new Exception("O formato de imagem precisa ser JPG.");
                    else if (model.ProfileImage.IsFileSizeInvalid())
                        throw new Exception("O tamanho da imagem não pode passar de 2MB");

                    bool hasDeleted = ImageService.DeleteOldProfileImage(userDb.profile_image ?? string.Empty);
                    string? imagePath = null;

                    if (hasDeleted || string.IsNullOrEmpty(userDb.profile_image))
                        imagePath = await model.ProfileImage.SaveProfileImageAsync();

                    if (!string.IsNullOrEmpty(imagePath))
                        model.UpdateUser.profile_image_path = imagePath;
                }

                await _userRepository.UpdateUser(model.UpdateUser);
                TempData["OkMsg"] = "Usuário atualizado com sucesso.";
                return RedirectToAction("Profile", "User");
            }
            else if (action == "delete")
            {
                var userDb = await _userRepository.getUserById(userId.Value);
                if (userDb is null)
                {
                    TempData["ErrorMsg"] = "Usuário não encontrado.";
                    return View(model);
                }

                if (!PasswordUtils.VerifyPassword(model.DeleteUser.password, userDb.password))
                {
                    TempData["ErrorMsg"] = "Senha inválida.";
                    return View(model);
                }

                if (!model.DeleteUser.confirm_delete)
                {
                    TempData["ErrorMsg"] = "Confirme a exclusão.";
                    return View(model);
                }

                await _userRepository.DeleteUser(userId.Value);
                CookiesHelper.deleteAuthCookie(Response);
                TempData["OkMsg"] = "Usuário excluído com sucesso.";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (IOException)
        {
            TempData["ErrorMsg"] = $"Ocorreu um erro ao fazer o upload da imagem.";
            return RedirectToAction("Profile", "User");
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMsg"] = $"Houve um erro em atualizar o usuário";
            return RedirectToAction("Profile", "User");
        }
        catch (Exception e)
        {
            var msg = e.Message;

            if (!string.IsNullOrEmpty(msg))
            {
                TempData["ErrorMsg"] = $"{msg}";
                return RedirectToAction("Profile", "User");
            }

            TempData["ErrorMsg"] = $"Houve um erro em atualizar o usuário";
            return RedirectToAction("Profile", "User");
        }

        return RedirectToAction("Index", "Home");
    }
}


