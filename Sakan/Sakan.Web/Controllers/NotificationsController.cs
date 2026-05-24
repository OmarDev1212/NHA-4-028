using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sakan.Application.Services.Notifications;
using System.Security.Claims;

namespace Sakan.Web.Controllers;

[Authorize]
public class NotificationsController(INotificationService notificationService) : Controller
{
    public async Task<IActionResult> Index(bool unreadOnly = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notifications = await notificationService.GetInboxAsync(userId, unreadOnly);
        ViewBag.UnreadCount = await notificationService.GetUnreadCountAsync(userId);
        ViewBag.UnreadOnly = unreadOnly;
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await notificationService.MarkAsReadAsync(id, userId);
        return RedirectToAction(nameof(Index));
    }
}
