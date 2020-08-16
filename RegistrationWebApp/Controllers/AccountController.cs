using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RegistrationWebApp.Models;
using RegistrationWebApp.ViewModels;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace RegistrationWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly ILogger<RegisterViewModel> logger;

        public AccountController(UserManager<User> userManager, 
            SignInManager<User> signInManager, ILogger<RegisterViewModel> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = new User { UserName = model.UserName, Email = model.Email };
                var result = await userManager.CreateAsync(user, model.Password);
                return await CheckRegistrationResult(result, model, user);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null) => 
            View(new LoginViewModel { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.UserName);
                SignInResult result = await TryLogIn(user, model);
                return await CheckLogInResult(result, model, user);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult> CheckRegistrationResult(IdentityResult result, RegisterViewModel model, User user)
        {
            if (result.Succeeded)
            {
                logger.LogInformation("User created a new account.");
                await signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        private async Task<SignInResult> TryLogIn(User user, LoginViewModel model) => 
            user != null && user.IsBlocked ? SignInResult.LockedOut : 
            await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

        private async Task<IActionResult> CheckLogInResult(SignInResult result, LoginViewModel model, User user)
        {
            if (result.Succeeded)
            {
                await SetLoginDate(user);
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                else return RedirectToAction("Index", "Home");
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "User was blocked.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        private async Task SetLoginDate(User user)
        {
            if (user != null)
            {
                user.LoginDate = DateTime.Now;
                await userManager.UpdateAsync(user);
            }
        }
    }
}
