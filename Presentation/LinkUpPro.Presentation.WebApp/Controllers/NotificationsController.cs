using System.Security.Claims;
using LinkUpPro.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Presentation.WebApp.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(currentUserId);
        ViewBag.UnreadCount = await _notificationService.GetUnreadCountAsync(currentUserId);

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id, string? redirectUrl = null)
    {
        var currentUserId = GetCurrentUserId();
        await _notificationService.MarkAsReadAsync(id, currentUserId);

        if (!string.IsNullOrWhiteSpace(redirectUrl) && Url.IsLocalUrl(redirectUrl))
        {
            return Redirect(redirectUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var currentUserId = GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(currentUserId);

        TempData["SuccessMessage"] = "Todas las notificaciones han sido marcadas como leídas.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetCounts()
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Json(new { unreadNotifications = 0, pendingRequests = 0 });
        }

        var unreadNotifications = await _notificationService.GetUnreadCountAsync(currentUserId);
        var pendingRequests = await _notificationService.GetPendingFriendRequestsCountAsync(currentUserId);

        return Json(new { unreadNotifications, pendingRequests });
    }
}
