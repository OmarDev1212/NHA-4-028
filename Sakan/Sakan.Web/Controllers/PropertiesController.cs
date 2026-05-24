using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sakan.Application.DTO;
using Sakan.Application.Services.PropertyService;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;

namespace Sakan.Web.Controllers;

public class PropertiesController(IPropertyService propertyService, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var properties = (await propertyService.GetAllAsync()).ToList();
        await EnrichOwnerEmailsAsync(properties);
        return View(properties); 
   }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id is null)
            return NotFound();

        var property = await propertyService.GetByIdAsync(id.Value);
        if (property is null)
            return NotFound();

        return View(property);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateViewDataAsync();
        return View(new PropertyDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyDto dto, string? featuresText)
    {
        ApplyFeaturesText(dto, featuresText);

        if (string.IsNullOrEmpty(dto.CurrentOwnerId))
            ModelState.AddModelError(nameof(PropertyDto.CurrentOwnerId), "Owner is required.");

        if (!ModelState.IsValid)
        {
            await PopulateViewDataAsync(dto.CurrentOwnerId, dto.PropertyType, dto.LegalStatus);
            return View(dto);
        }

        await propertyService.CreateAsync(dto, dto.CurrentOwnerId!);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id is null)
            return NotFound();

        var property = await propertyService.GetByIdAsync(id.Value);
        if (property is null)
            return NotFound();

        await PopulateViewDataAsync(property.CurrentOwnerId, property.PropertyType, property.LegalStatus);
        return View(property);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PropertyDto dto, string? featuresText)
    {
        if (id != dto.Id)
            return NotFound();

        ApplyFeaturesText(dto, featuresText);

        if (string.IsNullOrEmpty(dto.CurrentOwnerId))
            ModelState.AddModelError(nameof(PropertyDto.CurrentOwnerId), "Owner is required.");

        if (!ModelState.IsValid)
        {
            await PopulateViewDataAsync(dto.CurrentOwnerId, dto.PropertyType, dto.LegalStatus);
            return View(dto);
        }

        var updated = await propertyService.UpdateAsync(dto);
        if (!updated)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id is null)
            return NotFound();

        var property = await propertyService.GetByIdAsync(id.Value);
        if (property is null)
            return NotFound();

        return View(property);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var deleted = await propertyService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    private static void ApplyFeaturesText(PropertyDto dto, string? featuresText)
    {
        if (featuresText is null)
            return;

        dto.Features = featuresText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private async Task PopulateViewDataAsync(
        string? selectedOwnerId = null,
        string? selectedPropertyType = null,
        string? selectedLegalStatus = null)
    {
        var users = await userManager.Users
            .Select(u => new { u.Id, Name = u.Email ?? u.UserName ?? u.Id })
            .ToListAsync();

        ViewBag.CurrentOwnerId = new SelectList(users, "Id", "Name", selectedOwnerId);
        ViewBag.PropertyType = new SelectList(Enum.GetNames<PropertyType>(), selectedPropertyType);
        ViewBag.LegalStatus = new SelectList(Enum.GetNames<LegalStatus>(), selectedLegalStatus);
    }


    private async Task EnrichOwnerEmailsAsync(IEnumerable<PropertyDto> properties)
    {
        var ownerIds = properties
            .Select(p => p.CurrentOwnerId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        if (ownerIds.Count == 0)
            return;

        var emailsById = await userManager.Users
            .Where(u => ownerIds.Contains(u.Id))
            .Select(u => new { u.Id, Display = u.Email ?? u.UserName ?? u.Id })
            .ToDictionaryAsync(x => x.Id, x => x.Display);

        foreach (var property in properties)
        {
            if (property.CurrentOwnerId is not null
                && emailsById.TryGetValue(property.CurrentOwnerId, out var email))
            {
                property.CurrentOwnerEmail = email;
            }
        }
    }
}
