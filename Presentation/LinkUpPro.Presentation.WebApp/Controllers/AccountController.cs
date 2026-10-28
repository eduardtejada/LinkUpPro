using System.Security.Claims;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Presentation.WebApp.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    private string GetOriginUrl() => $"{Request.Scheme}://{Request.Host}";

    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    #region Registro

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm, IFormFile? profilePicture)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        Stream? pictureStream = null;
        string? pictureFileName = null;

        if (profilePicture != null && profilePicture.Length > 0)
        {
            pictureStream = profilePicture.OpenReadStream();
            pictureFileName = profilePicture.FileName;
        }

        var result = await _accountService.RegisterAsync(vm, pictureStream, pictureFileName, GetOriginUrl());

        if (result.HasError)
        {
            ViewBag.ErrorMessage = result.Error;
            return View(vm);
        }

        TempData["SuccessMessage"] = result.SuccessMessage;
        return RedirectToAction(nameof(Login));
    }

    #endregion

    #region Activación de Cuenta

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _accountService.ConfirmAccountAsync(userId, token);
        ViewBag.HasError = result.HasError;
        ViewBag.Message = result.HasError ? result.Error : result.SuccessMessage;

        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResendActivation(string? email = null)
    {
        return View(new ResendActivationViewModel { Email = email ?? string.Empty });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendActivation(ResendActivationViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _accountService.ResendActivationEmailAsync(vm.Email, GetOriginUrl());

        if (result.HasError)
        {
            ViewBag.ErrorMessage = result.Error;
            return View(vm);
        }

        ViewBag.SuccessMessage = result.SuccessMessage;
        return View(vm);
    }

    #endregion

    #region Inicio y Cierre de Sesión

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _accountService.LoginAsync(vm);

        if (result.HasError)
        {
            ViewBag.ErrorMessage = result.Error;
            ViewBag.IsLockedOut = result.IsLockedOut;
            ViewBag.IsAccountInactive = result.IsAccountInactive;
            ViewBag.InactiveEmail = !string.IsNullOrWhiteSpace(result.InactiveEmail) 
                ? result.InactiveEmail 
                : vm.UserNameOrEmail;

            return View(vm);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    #endregion

    #region Recuperación de Contraseña

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _accountService.ForgotPasswordAsync(vm, GetOriginUrl());
        ViewBag.SuccessMessage = result.SuccessMessage;

        return View(vm);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _accountService.ResetPasswordAsync(vm);

        if (result.HasError)
        {
            ViewBag.ErrorMessage = result.Error;
            return View(vm);
        }

        TempData["SuccessMessage"] = result.SuccessMessage;
        return RedirectToAction(nameof(Login));
    }

    #endregion

    #region Perfil de Usuario

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        var profile = await _accountService.GetProfileAsync(userId);

        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel vm, IFormFile? profilePicture)
    {
        var userId = GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        Stream? pictureStream = null;
        string? pictureFileName = null;

        if (profilePicture != null && profilePicture.Length > 0)
        {
            pictureStream = profilePicture.OpenReadStream();
            pictureFileName = profilePicture.FileName;
        }

        var result = await _accountService.UpdateProfileAsync(userId, vm, pictureStream, pictureFileName);

        if (result.HasError)
        {
            ViewBag.ErrorMessage = result.Error;
            return View(vm);
        }

        ViewBag.SuccessMessage = result.SuccessMessage;

        // Recargar datos actualizados
        var updatedProfile = await _accountService.GetProfileAsync(userId);
        return View(updatedProfile ?? vm);
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var userId = GetCurrentUserId();
        var result = await _accountService.ChangePasswordAsync(userId, vm);

        if (result.HasError)
        {
            ViewBag.ErrorMessage = result.Error;
            return View(vm);
        }

        TempData["SuccessMessage"] = result.SuccessMessage;
        return RedirectToAction(nameof(Profile));
    }

    #endregion
}
