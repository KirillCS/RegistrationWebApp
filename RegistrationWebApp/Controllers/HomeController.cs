using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RegistrationWebApp.Models;
using RegistrationWebApp.ViewModels;

namespace RegistrationWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public HomeController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index() => View(userManager.Users);

        [HttpPost]
        public async Task<IActionResult> Manage(string actionName, List<string> ids)
        {
            await SignOutIfBlocked();
            await DoAction(SetAction(actionName), ids, actionName.ToLower() == "delete" || actionName.ToLower() == "block");
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
       
        private async Task DoAction(Func<User, Task<IdentityResult>> action, List<string> ids, bool signOut)
        {
            User user;
            foreach (string id in ids)
            {
                user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result = await action(user);
                    await CheckActionResult(result, user, signOut);
                }
            }
        }
        
        private async Task CheckActionResult(IdentityResult result, User user, bool signOut)
        {
            if (result.Succeeded)
            {
                if (signOut && User.Identity.Name == user.UserName) await signInManager.SignOutAsync();
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

        }

        private Func<User, Task<IdentityResult>> SetAction(string actionName)
        {
            string lowerName = actionName.ToLower();
            if (lowerName == "delete")
            {
                return async user => await userManager.DeleteAsync(user);
            }
            else if (actionName.ToLower() == "block")
            {
                return async user =>
                {
                    user.IsBlocked = true;
                    return await userManager.UpdateAsync(user);
                };
            }
            else if (actionName.ToLower() == "unblock")
            {
                return async user =>
                {
                    user.IsBlocked = false;
                    return await userManager.UpdateAsync(user);
                };
            }
            return async user => await Task.Run(() => IdentityResult.Failed(new IdentityError { Description = $"No action \"{actionName}\"" }));
        }

        private async Task SignOutIfBlocked()
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            if(user != null && user.IsBlocked == true)
            {
                await signInManager.SignOutAsync();
            }
        }
    }
}
