using System.Security.Claims;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModels.Friend;
using LinkUpPro.Core.Application.ViewModels.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Presentation.WebApp.Controllers;

[Authorize]
public class FriendsController : Controller
{
    private readonly IFriendshipService _friendshipService;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(IFriendshipService friendshipService, ILogger<FriendsController> logger)
    {
        _friendshipService = friendshipService;
        _logger = logger;
    }

    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] PostFilterViewModel? filter)
    {
        var currentUserId = GetCurrentUserId();
        var model = await _friendshipService.GetFriendsIndexAsync(currentUserId, filter);

        ViewBag.CurrentUserId = currentUserId;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Requests()
    {
        var currentUserId = GetCurrentUserId();
        var model = await _friendshipService.GetRequestsPageAsync(currentUserId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendRequest(SendFriendRequestViewModel vm)
    {
        var currentUserId = GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Debe especificar un nombre de usuario válido.";
            return RedirectToAction(nameof(Requests));
        }

        var result = await _friendshipService.SendRequestAsync(currentUserId, vm.UserName);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = $"Solicitud de amistad enviada a @{vm.UserName} con éxito.";
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _friendshipService.AcceptRequestAsync(id, currentUserId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "¡Solicitud aceptada! Ahora son amigos.";
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _friendshipService.RejectRequestAsync(id, currentUserId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Solicitud rechazada.";
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _friendshipService.CancelRequestAsync(id, currentUserId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Solicitud cancelada.";
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hide(int id)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _friendshipService.HideRequestAsync(id, currentUserId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Solicitud ocultada del historial.";
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string friendId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _friendshipService.RemoveFriendAsync(currentUserId, friendId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Amigo eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> MutualFriends(string userId)
    {
        var currentUserId = GetCurrentUserId();
        var mutuals = await _friendshipService.GetMutualFriendsAsync(currentUserId, userId);

        return Json(mutuals);
    }
}
