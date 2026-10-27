using System.Security.Claims;
using System.Text.Json;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModels.Battleship;
using LinkUpPro.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Presentation.WebApp.Controllers;

[Authorize]
public class BattleshipController : Controller
{
    private readonly IBattleshipService _battleshipService;
    private readonly ILogger<BattleshipController> _logger;

    public BattleshipController(
        IBattleshipService battleshipService,
        ILogger<BattleshipController> logger)
    {
        _battleshipService = battleshipService;
        _logger = logger;
    }

    private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private bool IsAjaxRequest() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = GetCurrentUserId();
        var model = await _battleshipService.GetIndexViewModelAsync(currentUserId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Challenge(string opponentId)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(opponentId))
        {
            TempData["ErrorMessage"] = "Debes seleccionar un oponente válido.";
            return RedirectToAction(nameof(Index));
        }

        var (success, errorMessage, gameId) = await _battleshipService.CreateGameChallengeAsync(currentUserId, opponentId);
        if (!success || !gameId.HasValue)
        {
            TempData["ErrorMessage"] = errorMessage ?? "No se pudo crear el reto de partida.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "¡Reto iniciado! Configura tus 5 barcos para comenzar.";
        return RedirectToAction(nameof(Placement), new { id = gameId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Placement(int id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, model) = await _battleshipService.GetPlacementViewModelAsync(id, currentUserId);

        if (!success || model == null)
        {
            TempData["ErrorMessage"] = errorMessage ?? "No se pudo acceder a la partida.";
            return RedirectToAction(nameof(Index));
        }

        // Si ambos jugadores ya están listos, redirigir a la pantalla de combate
        if (model.IsReady && model.OpponentReady)
        {
            return RedirectToAction(nameof(Play), new { id });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePlacement(SavePlacementInputModel input)
    {
        var currentUserId = GetCurrentUserId();

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(input.ShipsJson))
        {
            TempData["ErrorMessage"] = "Los datos de colocación de la flota están incompletos.";
            return RedirectToAction(nameof(Placement), new { id = input.GameId });
        }

        List<ShipPlacementDto>? placements;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            placements = JsonSerializer.Deserialize<List<ShipPlacementDto>>(input.ShipsJson, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al deserializar colocación de barcos para partida {GameId}", input.GameId);
            TempData["ErrorMessage"] = "El formato de los datos de los barcos es inválido.";
            return RedirectToAction(nameof(Placement), new { id = input.GameId });
        }

        if (placements == null || placements.Count != 5)
        {
            TempData["ErrorMessage"] = "Debes posicionar exactamente los 5 barcos reglamentarios.";
            return RedirectToAction(nameof(Placement), new { id = input.GameId });
        }

        var (success, errorMessage) = await _battleshipService.SavePlacementAsync(currentUserId, input.GameId, placements);
        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "No se pudo guardar la posición de la flota.";
            return RedirectToAction(nameof(Placement), new { id = input.GameId });
        }

        TempData["SuccessMessage"] = "¡Flota confirmada! Si tu oponente ya está listo, el combate comenzará de inmediato.";
        return RedirectToAction(nameof(Placement), new { id = input.GameId });
    }

    [HttpGet]
    public async Task<IActionResult> Play(int id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, model) = await _battleshipService.GetPlayViewModelAsync(id, currentUserId);

        if (!success || model == null)
        {
            TempData["ErrorMessage"] = errorMessage ?? "No se pudo cargar la partida.";
            return RedirectToAction(nameof(Index));
        }

        if (model.Status == GameStatus.Placement)
        {
            return RedirectToAction(nameof(Placement), new { id });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Attack(BattleshipAttackInputModel input)
    {
        var currentUserId = GetCurrentUserId();

        if (!ModelState.IsValid)
        {
            if (IsAjaxRequest())
            {
                return Json(new BattleshipAttackResultDto { Success = false, Message = "Coordenadas de disparo no válidas." });
            }

            TempData["ErrorMessage"] = "Coordenadas de disparo no válidas.";
            return RedirectToAction(nameof(Play), new { id = input.GameId });
        }

        var result = await _battleshipService.ExecuteAttackAsync(currentUserId, input.GameId, input.Row, input.Col);

        if (IsAjaxRequest())
        {
            return Json(result);
        }

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["AttackMessage"] = result.Message;
        }

        if (result.IsGameOver)
        {
            return RedirectToAction(nameof(HistoryDetails), new { id = input.GameId });
        }

        return RedirectToAction(nameof(Play), new { id = input.GameId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Surrender(int id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage) = await _battleshipService.SurrenderGameAsync(currentUserId, id);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "No se pudo procesar la rendición.";
            return RedirectToAction(nameof(Play), new { id });
        }

        TempData["SuccessMessage"] = "Te has rendido de la partida. La victoria ha sido asignada al oponente.";
        return RedirectToAction(nameof(HistoryDetails), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> HistoryDetails(int id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, errorMessage, model) = await _battleshipService.GetHistoryViewModelAsync(id, currentUserId);

        if (!success || model == null)
        {
            TempData["ErrorMessage"] = errorMessage ?? "No se pudo obtener el historial de la partida.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }
}
