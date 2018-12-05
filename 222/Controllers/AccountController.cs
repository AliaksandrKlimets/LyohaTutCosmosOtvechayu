using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using _222.Models;
using _222.EF;
using _222.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace _222.Controllers
{
    public class AccountController : Controller
{
        readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else
            {
                var user = await _userService.FindOnLoginAsync(model.Email, model.PasswordString);
                if (user == null)
                {
                    ModelState.AddModelError("", "Incorrect email or password");
                    return View(model);
                }
                else
                {
                    var claims = new Claim[]
                    {
                        new Claim(ClaimTypes.Name,user.Name+" "+user.Surname),
                        new Claim(ClaimTypes.Email,user.Email)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            bool isEmailExist = await _userService.IsEmailExistAsync(model.Email);
            if (isEmailExist)
            {
                ModelState.AddModelError("Email", "This email is already exists");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else
            {
                _userService.CreateItem(new User { Email = model.Email, Name = model.Name, Surname = model.Surname, Password = model.PasswordString });
                return View("Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("Login");
        }
    }
}
