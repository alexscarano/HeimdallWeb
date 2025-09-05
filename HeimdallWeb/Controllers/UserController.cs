using HeimdallWeb.DTO;
using HeimdallWeb.Helpers;
using HeimdallWeb.Models;
using HeimdallWeb.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.Controllers;

public class UserController : Controller
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IActionResult Index()
    {
        return View();  
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult Delete()
    {
        return View();
    }

    public IActionResult History()
    {
        var users = _userRepository.getAllUsers() ?? throw new Exception("Ocorreu um erro ao consultar os usu�rios");

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

    public IActionResult Edit()
    {
        return View();
    }    

    [HttpPost]
    public IActionResult RegisterAction(UserModel user)
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (!_userRepository.verifyIfUserExists(user))
                {
                    _userRepository.insertUser(user);
                    TempData["OkMsg"] = "O usu�rio foi cadastrado com sucesso";
                    return RedirectToAction("Index", "Home");
                }
                TempData["ErrorMsg"] = "Este usu�rio j� est� cadastrado, verique o seu email/nome de usu�rio";
            }
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Ocorreu um erro no cadastro do usu�rio";
        }
        return View("Register", user);
    }

    [HttpPost]
    public IActionResult EditAction(UserModel user)
    {
        try
        {
            ModelState.Remove(nameof(user.user_id));

            if (ModelState.IsValid)
            {
                 //user.user_id = 1;
                _userRepository.updateUser(user);
                TempData["OkMsg"] = "Usu�rio atualizado com sucesso";
                return View("Edit", user);
            }
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Ocorreu um erro ao atualizar o usu�rio";
        }
        return View("Edit", user); 
    }

    public IActionResult DeleteAction(int id, DeleteUserDTO userToDelete)
    {
        try
        {
            id = 16; // colocar sess�o

            var userDB = _userRepository.getUserById(id) ?? throw new Exception("N�o foi poss�vel fazer a consulta do usu�rio");

            if (PasswordUtils.VerifyPassword(userToDelete.password, userDB.password))
            {
                bool deleted = _userRepository.deleteUser(id);

                if (deleted)
                {
                    TempData["OkMsg"] = "Usu�rio deletado com sucesso, volte sempre.";
                }
            }
        }
        catch (Exception)
        {
            TempData["ErrorMsg"] = "Ocorreu um erro ao tentar deletar o usu�rio, tente novamente.";
            return View("Delete");
        }

        return RedirectToAction("Index", "Login");
    }

}