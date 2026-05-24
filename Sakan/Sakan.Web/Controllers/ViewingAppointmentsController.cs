using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sakan.Application.DTO;
using Sakan.Application.Services.ViewingAppointments;
using Sakan.Domain.Entities;
using Sakan.Domain.Enums;
using System.Security.Claims;

namespace Sakan.Web.Controllers;

[Authorize]
public class ViewingAppointmentsController(
    IViewingAppointmentService appointmentService,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index(bool asAgent = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var appointments = await appointmentService.GetForUserAsync(userId, asAgent);
        ViewBag.AsAgent = asAgent;
        return View(appointments);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var appointment = await appointmentService.GetByIdAsync(id);
        if (appointment is null)
            return NotFound();
        return View(appointment);
    }

    [HttpGet]
    public IActionResult Create(Guid listingId)
    {
        return View(new CreateViewingAppointmentDto
        {
            ListingId = listingId,
            ScheduledAt = DateTimeOffset.UtcNow.AddDays(1),
            MeetingType = MeetingType.Virtual
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateViewingAppointmentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!ModelState.IsValid)
            return View(dto);

        var result = await appointmentService.RequestAsync(dto, userId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not create appointment.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var appointment = await appointmentService.GetByIdAsync(id);
        if (appointment is null)
            return NotFound();

        await PopulateAgentsAsync(appointment.AgentId);
        return View(new ConfirmViewingAppointmentDto { AgentId = appointment.AgentId ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid id, ConfirmViewingAppointmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAgentsAsync(dto.AgentId);
            return View(dto);
        }

        var result = await appointmentService.ConfirmAsync(id, dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not confirm appointment.");
            await PopulateAgentsAsync(dto.AgentId);
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await appointmentService.CompleteAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Appointment completed." : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await appointmentService.CancelAsync(id, userId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Appointment cancelled." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAgentsAsync(string? selectedId)
    {
        var agents = await userManager.GetUsersInRoleAsync("Agent");
        var items = agents.Select(a => new { a.Id, Name = a.Email ?? a.UserName ?? a.Id }).ToList();
        ViewBag.AgentId = new SelectList(items, "Id", "Name", selectedId);
    }
}
