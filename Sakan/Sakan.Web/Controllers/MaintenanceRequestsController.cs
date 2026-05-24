using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sakan.Application.DTO;
using Sakan.Application.Services.MaintenanceRequests;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;
using System.Security.Claims;

namespace Sakan.Web.Controllers;

[Authorize]
public class MaintenanceRequestsController(
    IMaintenanceRequestService maintenanceService,
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var requests = await maintenanceService.GetForUserAsync(userId);
        return View(requests);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var request = await maintenanceService.GetByIdAsync(id);
        if (request is null)
            return NotFound();
        return View(request);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? propertyId)
    {
        await PopulatePropertiesAsync(propertyId);
        return View(new CreateMaintenanceRequestDto
        {
            PropertyId = propertyId ?? Guid.Empty,
            Priority = Priority.Medium
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMaintenanceRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!ModelState.IsValid)
        {
            await PopulatePropertiesAsync(dto.PropertyId);
            return View(dto);
        }

        var result = await maintenanceService.CreateAsync(dto, userId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not create request.");
            await PopulatePropertiesAsync(dto.PropertyId);
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Assign(Guid id)
    {
        var request = await maintenanceService.GetByIdAsync(id);
        if (request is null)
            return NotFound();

        await PopulateContractorsAsync(request.AssignedToId);
        return View(new AssignMaintenanceRequestDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(Guid id, AssignMaintenanceRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!ModelState.IsValid)
        {
            await PopulateContractorsAsync(dto.AssignedToId);
            return View(dto);
        }

        var result = await maintenanceService.AssignAsync(id, dto, userId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not assign contractor.");
            await PopulateContractorsAsync(dto.AssignedToId);
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(Guid id, decimal? actualCost)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await maintenanceService.ResolveAsync(id, actualCost, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Request resolved." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulatePropertiesAsync(Guid? selectedId)
    {
        var properties = (await unitOfWork.GetRepository<Property, Guid>()
            .GetAllAsync(p => !p.IsDeleted))
            .Select(p => new { p.Id, Name = p.Title })
            .ToList();
        ViewBag.PropertyId = new SelectList(properties, "Id", "Name", selectedId);
        ViewBag.Category = new SelectList(Enum.GetNames<MaintenanceCategory>());
    }

    private async Task PopulateContractorsAsync(string? selectedId)
    {
        var users = await userManager.Users
            .Select(u => new { u.Id, Name = u.Email ?? u.UserName ?? u.Id })
            .ToListAsync();
        ViewBag.AssignedToId = new SelectList(users, "Id", "Name", selectedId);
    }
}
