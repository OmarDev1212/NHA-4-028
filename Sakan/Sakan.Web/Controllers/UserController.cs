using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sakan.Domain.Entities;
using Sakan.Web.ViewModels;

namespace Sakan.Web.Controllers
{
    public class UserController(UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, ILogger<UserController> _logger, RoleManager<IdentityRole> _roleManager) : Controller
    {
        public async Task<IActionResult> Index(string? searchValue)
        {
            IQueryable<ApplicationUser> query = _userManager.Users;
            if (!string.IsNullOrEmpty(searchValue))
                query = query.Where(u => u.Email.Contains(searchValue));
            var userEntities = await query.ToListAsync();
            var users = new List<UserViewModel>();
            foreach (var u in userEntities)
            {
                users.Add(new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    Roles = await _userManager.GetRolesAsync(u)
                });
            }
            return View(users);


        }


        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return BadRequest();
            var userToReturn = new UserViewModel()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Roles = _userManager.GetRolesAsync(user).Result
            };
            return View(userToReturn);
        }
        public async Task<IActionResult> Edit(string? id)
        {

            if (id is null) return BadRequest();
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();
            var userToReturn = new UserViewModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = await _userManager.GetRolesAsync(user)
            };
            return View(userToReturn);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] string id, UserViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null) return NotFound();
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            // Reassign roles
            var existingRoles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove existing roles.");
                return View(model);
            }

            if (model.Roles != null && model.Roles.Any())
            {
                // Convert role IDs to role names
                var roleNames = await _roleManager.Roles
                    .Where(r => model.Roles.Contains(r.Id))
                    .Select(r => r.Name)
                    .ToListAsync();

                var addResult = await _userManager.AddToRolesAsync(user, roleNames);
                if (!addResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to assign new roles.");
                    return View(model);
                }
            }
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                if (_environment.IsDevelopment())
                {
                    TempData["ErrorMessage"] = "Something went wrong";
                    ModelState.AddModelError(string.Empty, "Data did not ");
                }
                else
                {
                    TempData["ErrorMessage"] = "Something went wrong";
                    _logger.LogError("Editing Data is not completed successfully");
                }
                return View(model);
            }
        }
        public async Task<IActionResult> Delete(string? id)
        {
            if (id is null) return BadRequest();
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();
            var userToReturn = new UserViewModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = await _userManager.GetRolesAsync(user)
            };
            return View(userToReturn);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            if (id == null) return BadRequest();
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                if (_environment.IsDevelopment())
                {
                    TempData["ErrorMessage"] = "Something went wrong";
                    ModelState.AddModelError(string.Empty, " Data is not deleted successfully");
                }
                else
                {
                    TempData["ErrorMessage"] = "Something went wrong";
                    _logger.LogError("Data is not deleted successfully");
                }
                ModelState.AddModelError("", "Invalid Operation");
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}