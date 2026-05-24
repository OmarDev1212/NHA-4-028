using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sakan.Web.ViewModels;

namespace Sakan.Web.Controllers
{
    public class RoleController(RoleManager<IdentityRole> _roleManager, IWebHostEnvironment _environment, ILogger<RoleController> _logger) : Controller
    {
        public async Task<IActionResult> Index(string? searchValue)
        {
            IEnumerable<RoleViewModel> roles;
            if (string.IsNullOrEmpty(searchValue))
            {
                roles = _roleManager.Roles.Select(r => new RoleViewModel()
                {
                    Id = r.Id,
                    Name = r.Name
                });
            }
            else
            {
                roles = _roleManager.Roles.Where(r => r.Name.ToLower().Contains(searchValue.ToLower())).Select(

                    r => new RoleViewModel()
                    {
                        Id = r.Id,
                        Name = r.Name
                    });
            }
            return View(roles);

        }

        public async Task<IActionResult> Details(string id)
        {
            if (id is null) return BadRequest();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            var roleToReturn = new RoleViewModel()
            {
                Id = role.Id,
                Name = role.Name
            };
            return View(roleToReturn);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var role = new IdentityRole()
            {
                Name = model.Name,
            };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Role created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(string? id)
        {

            if (id is null) return BadRequest();
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null) return NotFound();
            var roleToReturn = new RoleViewModel()
            {
                Id = role.Id,
                Name = role.Name
            };
            return View(roleToReturn);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] string id, RoleViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role is null) return NotFound();
            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Role updated successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                if (_environment.IsDevelopment())
                {
                    TempData["ErrorMessage"] = "Something went wrong";
                    ModelState.AddModelError(string.Empty, "Data did not save successfully");
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
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null) return NotFound();
            var roleToReturn = new RoleViewModel()
            {
                Id = role.Id,
                Name = role.Name
            };
            return View(roleToReturn);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            if (id == null) return BadRequest();
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null) return NotFound();
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Role Deleted successfully";
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