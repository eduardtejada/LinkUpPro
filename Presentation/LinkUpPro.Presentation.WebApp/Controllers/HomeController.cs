using System.Diagnostics;
using System.Security.Claims;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModels.Comment;
using LinkUpPro.Core.Application.ViewModels.Post;
using LinkUpPro.Core.Domain.Enums;
using LinkUpPro.Presentation.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Presentation.WebApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IPostService _postService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IPostService postService, ILogger<HomeController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] PostFilterViewModel? filter)
    {
        var currentUserId = GetCurrentUserId();
        var posts = await _postService.GetHomeFeedAsync(currentUserId, filter);

        ViewBag.Filter = filter ?? new PostFilterViewModel();
        ViewBag.CurrentUserId = currentUserId;

        return View(posts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SavePostViewModel vm, IFormFile? imageFile)
    {
        var currentUserId = GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Por favor verifica los campos de la publicación.";
            return RedirectToAction(nameof(Index));
        }

        Stream? imageStream = null;
        string? imageFileName = null;

        if (imageFile != null && imageFile.Length > 0)
        {
            imageStream = imageFile.OpenReadStream();
            imageFileName = imageFile.FileName;
        }

        var result = await _postService.CreatePostAsync(currentUserId, vm, imageStream, imageFileName);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Publicación creada exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var currentUserId = GetCurrentUserId();
        var post = await _postService.GetForEditAsync(id, currentUserId);

        if (post == null)
        {
            TempData["ErrorMessage"] = "No se encontró la publicación o no tienes permisos para editarla.";
            return RedirectToAction(nameof(Index));
        }

        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SavePostViewModel vm, IFormFile? imageFile)
    {
        var currentUserId = GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        Stream? imageStream = null;
        string? imageFileName = null;

        if (imageFile != null && imageFile.Length > 0)
        {
            imageStream = imageFile.OpenReadStream();
            imageFileName = imageFile.FileName;
        }

        var result = await _postService.UpdatePostAsync(currentUserId, vm, imageStream, imageFileName);

        if (!result.Success)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;
            return View(vm);
        }

        TempData["SuccessMessage"] = "Publicación actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        var success = await _postService.DeletePostAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar la publicación.";
        }
        else
        {
            TempData["SuccessMessage"] = "Publicación eliminada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComments(int id)
    {
        var currentUserId = GetCurrentUserId();
        var success = await _postService.ToggleCommentsAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = "No se pudo actualizar el estado de los comentarios.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(SaveCommentViewModel vm)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(vm.Content))
        {
            TempData["ErrorMessage"] = "El comentario no puede estar vacío.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _postService.AddCommentAsync(currentUserId, vm);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var currentUserId = GetCurrentUserId();
        var success = await _postService.DeleteCommentAsync(id, currentUserId);

        if (!success)
        {
            TempData["ErrorMessage"] = "No se pudo eliminar el comentario.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleReaction(int postId, ReactionType type)
    {
        var currentUserId = GetCurrentUserId();
        await _postService.ToggleReactionAsync(postId, currentUserId, type);

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
