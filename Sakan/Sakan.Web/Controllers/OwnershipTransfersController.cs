using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sakan.Application.DTO;
using Sakan.Application.Services.Documents;
using Sakan.Application.Services.OwnershipTransfers;
using Sakan.Domain.Constants;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;
using System.Security.Claims;

namespace Sakan.Web.Controllers;

[Authorize]
public class OwnershipTransfersController(
    IOwnershipTransferService transferService,
    IDocumentService documentService,
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var transfers = await transferService.GetForUserAsync(userId);
        return View(transfers);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var transfer = await transferService.GetByIdAsync(id);
        if (transfer is null)
            return NotFound();

        var documents = await documentService.GetForEntityAsync(id, EntityReferenceTypes.OwnershipTransfer);
        ViewBag.Documents = documents;
        return View(transfer);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? propertyId)
    {
        await PopulateCreateFormAsync(propertyId);
        return View(new CreateOwnershipTransferDto { PropertyId = propertyId ?? Guid.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOwnershipTransferDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!ModelState.IsValid)
        {
            await PopulateCreateFormAsync(dto.PropertyId, dto.ToOwnerId);
            return View(dto);
        }

        var result = await transferService.InitiateAsync(dto, userId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not initiate transfer.");
            await PopulateCreateFormAsync(dto.PropertyId, dto.ToOwnerId);
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(Guid id, DocumentType documentType, IFormFile file)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await documentService.UploadAsync(
            id, EntityReferenceTypes.OwnershipTransfer, documentType, userId, file);

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Document uploaded." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitDocuments(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await transferService.SubmitDocumentsAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Documents submitted." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LegalReview(Guid id, LegalReviewDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await transferService.LegalReviewAsync(id, dto, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Legal review recorded." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEscrow(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await transferService.CreateEscrowPaymentAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Escrow funded." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkSigned(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await transferService.MarkSignedAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Marked as signed." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRegistered(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await transferService.MarkRegisteredAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Registry confirmed." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await transferService.CompleteAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Transfer completed." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, string reason)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await transferService.CancelAsync(id, userId, reason);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? "Transfer cancelled." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateCreateFormAsync(Guid? propertyId = null, string? selectedBuyerId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var properties = (await unitOfWork.GetRepository<Property, Guid>()
            .GetAllAsync(p => !p.IsDeleted && p.CurrentOwnerId == userId))
            .Select(p => new { p.Id, Name = p.Title })
            .ToList();

        var users = await userManager.Users
            .Select(u => new { u.Id, Name = u.Email ?? u.UserName ?? u.Id })
            .ToListAsync();

        ViewBag.PropertyId = new SelectList(properties, "Id", "Name", propertyId);
        ViewBag.ToOwnerId = new SelectList(users, "Id", "Name", selectedBuyerId);
        ViewBag.DocumentType = new SelectList(Enum.GetNames<DocumentType>());
    }
}
