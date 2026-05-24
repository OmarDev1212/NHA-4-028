using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;
using Sakan.Infrastructure.Identity;
using Sakan.Web.Models;

namespace Sakan.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    UserRoleAssignmentService roleAssignmentService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
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
        if (!identityResult.Succeeded)
        {
            foreach (var error in identityResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        await roleAssignmentService.EnsureMemberRoleAsync(user);
        await signInManager.SignInAsync(user, isPersistent: false);

        TempData["SuccessMessage"] = "Account created successfully.";
        return RedirectToAction(nameof(Onboarding));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Onboarding()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
            return Challenge();

        if (user.OnboardingCompleted)
            return RedirectToAction("Index", "Home");

        return View(new OnboardingViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onboarding(OnboardingViewModel model, string? skip)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
            return Challenge();

        if (!string.IsNullOrEmpty(skip))
        {
            user.OnboardingCompleted = true;
            await userManager.UpdateAsync(user);
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
            return View(model);

        user.OnboardingIntent = model.Intent;
        user.OnboardingCompleted = true;
        await userManager.UpdateAsync(user);

        TempData["WelcomeMessage"] = GetWelcomeMessage(model.Intent!.Value);
        return RedirectToHomeForIntent(model.Intent!.Value);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account is locked out.");
            return View(model);
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "You are not allowed to sign in.");
            return View(model);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        if (!user.OnboardingCompleted)
            return RedirectToAction(nameof(Onboarding));

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync() =>
        await userManager.GetUserAsync(User);

    private IActionResult RedirectToHomeForIntent(UserIntent intent) =>
        RedirectToAction("Index", "Home", new { intent = intent.ToString() });

    private static string GetWelcomeMessage(UserIntent intent) => intent switch
    {
        UserIntent.BrowseToBuy => "Welcome! Start browsing properties for sale.",
        UserIntent.BrowseToRent => "Welcome! Start browsing properties for rent.",
        UserIntent.ListForSale => "Welcome! You can list a property for sale when you are ready.",
        UserIntent.ListForRent => "Welcome! You can list a property for rent when you are ready.",
        _ => "Welcome to Sakan!",
    };
}
