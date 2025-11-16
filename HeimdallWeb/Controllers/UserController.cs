using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.Controllers;

public class UserController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    private async Task<string> SaveProfileImageAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return null;

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "users");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(uploadsFolder, uniqueName);

        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))

        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/users/{uniqueName}";
    }

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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit()
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
                // não preencha password/confirm_password por segurança
            },
            DeleteUser = new DeleteUserDTO()
        };

        return View(model);
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
    public async Task<IActionResult> Edit(UserEditViewModel model, string? action)
    {
        try
        {
            // 'action' vem do name do botão submit (ex.: name="action" value="update" ou "delete")
            int? userId = TokenService.GetUserIdFromClaims(User);
            if (userId is null)
            {
                TempData["ErrorMsg"] = "Erro de autenticação.";
                return View(model);
            }

            if (action == "update")
            {
                // garante que o user_id é o do token
                model.UpdateUser.user_id = userId.Value;

                // Verificações de negócio usando UpdateUser
                if (await _userRepository.VerifyIfUserExistsWithLogin(model.UpdateUser))
                {
                    TempData["ErrorMsg"] = "Já existe um usuário com este login, tente outro.";
                    return View(model);
                }

                if (await _userRepository.VerifyIfUserExistsWithEmail(model.UpdateUser))
                {
                    TempData["ErrorMsg"] = "Já existe um usuário com este email, tente outro.";
                    return View(model);
                }

                var userDb = await _userRepository.getUserById(userId.Value);
                if (userDb == null)
                {
                    TempData["ErrorMsg"] = "Usuário não encontrado.";
                    return View(model);
                }

                if (model.ProfileImage is not null)
                {
                    string? imagePath = await SaveProfileImageAsync(model.ProfileImage);
                    if (imagePath != null)
                    {
                        model.UpdateUser.profile_image_path = imagePath;
                    }
                }

                await _userRepository.UpdateUser(model.UpdateUser);
                TempData["OkMsg"] = "Usuário atualizado com sucesso.";
                return RedirectToAction("Index", "Home");
            }
            else if (action == "delete")
            {
                // valida apenas o DeleteUser
                if (!TryValidateModel(model.DeleteUser, prefix: "DeleteUser"))
                {
                    TempData["ErrorMsg"] = "Corrija os erros no formulário de exclusão.";
                    return View(model);
                }

                // confirmar senha (exemplo: verificar se senha bate com a do DB)
                var userDb = await _userRepository.getUserById(userId.Value);
                if (userDb == null)
                {
                    TempData["ErrorMsg"] = "Usuário não encontrado.";
                    return View(model);
                }

                // exemplo de método para verificar senha
                if (!PasswordUtils.VerifyPassword(userDb.password, model.DeleteUser.password))
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
                TempData["OkMsg"] = "Usuário excluído com sucesso.";
                return RedirectToAction("Index", "Home");
            }

            // Se action não vier ou for inválida, volta para a view com uma mensagem
            TempData["ErrorMsg"] = "Ação inválida.";
            return View(model);

        }
        catch (IOException e)
        {
            TempData["ErrorMsg"] = $"Ocorreu um erro ao fazer o upload da imagem. {e}";
            return View(model);
        }
        catch (UnauthorizedAccessException e)
        {
            TempData["ErrorMsg"] = $"Permissão negada ao salvar a imagem. {e}";
            return View(model);
        }
        catch (Exception e)
        {
            TempData["ErrorMsg"] = $"Deu merda aqui. {e}";
            return View(model);
        }
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