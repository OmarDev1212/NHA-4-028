using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sakan.Domain.Entities;
using Sakan.Web.Models;

namespace Sakan.Web.Controllers
{
    public class AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : Controller
    {
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };
            var identityResult = await userManager.CreateAsync(user, model.Password);
            if (identityResult.Succeeded)
            {
                TempData["SuccessMessage"] = "User Created successfully";
                return RedirectToAction(nameof(Login));
            }
            else
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                TempData["ErrorMessage"] = "Something went wrong";
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task< IActionResult> Login (LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null) ModelState.AddModelError("", "Invalid Login attempt");
            var flag = await userManager.CheckPasswordAsync(user, model.Password);
            if (flag)
            {
                var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.IsLockedOut)
                    ModelState.AddModelError("", "Account is lockedout");
                if (result.IsNotAllowed)
                    ModelState.AddModelError("", "You are not allowed to enter this site");

                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");


            }
            return View(model);
        }
    }
}
