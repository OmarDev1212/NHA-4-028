using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sakan.Application.DTO;
using Sakan.Application.Services.RentalApplications;
using System.Security.Claims;

namespace Sakan.Web.Controllers;

[Authorize]
public class RentalApplicationsController(IRentalApplicationService rentalService) : Controller
{
    public async Task<IActionResult> MyApplications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var applications = await rentalService.GetForApplicantAsync(userId);
        return View("Index", applications);
    }

    public async Task<IActionResult> ForListing(Guid listingId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var applications = await rentalService.GetForListingAsync(listingId, userId);
        ViewBag.ListingId = listingId;
        return View(applications);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var application = await rentalService.GetByIdAsync(id);
        if (application is null)
            return NotFound();
        return View(application);
    }

    [HttpGet]
    public IActionResult Create(Guid listingId)
    {
        return View(new CreateRentalApplicationDto
        {
            ListingId = listingId,
            RequestedFrom = DateOnly.FromDateTime(DateTime.Today),
            RequestedTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(12))
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRentalApplicationDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!ModelState.IsValid)
            return View(dto);

        var result = await rentalService.SubmitAsync(dto, userId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not submit application.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await rentalService.ApproveAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Application approved." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await rentalService.RejectAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Application rejected." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }
}
