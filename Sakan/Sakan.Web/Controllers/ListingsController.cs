using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sakan.Application.DTO;
using Sakan.Application.Services.PropertyListingService;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;
using System.Security.Claims;

namespace Sakan.Web.Controllers;

public class ListingsController(
    IPropertyListingService listingService,
    IUnitOfWork unitOfWork) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        ListingType? listingType,
        decimal? minPrice,
        decimal? maxPrice,
        DateOnly? availableFrom,
        DateOnly? availableTo,
        string? city,
        string? country)
    {
        var criteria = new ListingSearchCriteria
        {
            ListingType = listingType,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            AvailableFrom = availableFrom,
            AvailableTo = availableTo,
            City = city,
            Country = country,
            Status = ListingStatus.Active
        };

        ViewBag.ListingType = new SelectList(Enum.GetNames<ListingType>(), listingType?.ToString());
        ViewBag.City = city;
        ViewBag.Country = country;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.AvailableFrom = availableFrom?.ToString("yyyy-MM-dd");
        ViewBag.AvailableTo = availableTo?.ToString("yyyy-MM-dd");

        var listings = await listingService.SearchAsync(criteria);
        return View(listings);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id)
    {
        var listing = await listingService.GetByIdAsync(id);
        if (listing is null)
            return NotFound();

        await listingService.IncrementViewCountAsync(id);
        return View(listing);
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        await PopulatePropertiesAsync();
        return View(new CreatePropertyListingDto
        {
            AvailableFrom = DateOnly.FromDateTime(DateTime.Today),
            Currency = "USD"
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePropertyListingDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Challenge();

        if (!ModelState.IsValid)
        {
            await PopulatePropertiesAsync(dto.PropertyId);
            return View(dto);
        }

        var result = await listingService.CreateAsync(dto, userId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not create listing.");
            await PopulatePropertiesAsync(dto.PropertyId);
            return View(dto);
        }

        return RedirectToAction(nameof(MyListings));
    }

    [Authorize]
    public async Task<IActionResult> MyListings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Challenge();

        var all = await listingService.SearchAsync(new ListingSearchCriteria { Status = null });
        var mine = all.Where(l => l.ListedById == userId).ToList();
        return View(mine);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await listingService.PublishAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Listing published." : result.Error;
        return RedirectToAction(nameof(MyListings));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await listingService.WithdrawAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Listing withdrawn." : result.Error;
        return RedirectToAction(nameof(MyListings));
    }

    private async Task PopulatePropertiesAsync(Guid? selectedId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var allProperties = (await unitOfWork.GetRepository<Property, Guid>()
            .GetAllAsync(p => !p.IsDeleted && p.CurrentOwnerId == userId))
            .Select(p => new { p.Id, Name = p.Title })
            .ToList();

        ViewBag.PropertyId = new SelectList(allProperties, "Id", "Name", selectedId);
        ViewBag.ListingType = new SelectList(Enum.GetNames<ListingType>());
    }
}
