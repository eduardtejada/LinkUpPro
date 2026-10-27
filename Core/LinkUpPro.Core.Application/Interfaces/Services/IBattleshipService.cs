using LinkUpPro.Core.Application.ViewModels.Battleship;

namespace LinkUpPro.Core.Application.Interfaces.Services;

public interface IBattleshipService
{
    /// <summary>
    /// Carga el lobby principal de Battleship con partidas activas, historial y amigos disponibles.
    /// </summary>
    Task<BattleshipIndexViewModel> GetIndexViewModelAsync(string currentUserId);

    /// <summary>
    /// Crea un nuevo reto de partida contra un amigo activo.
    /// </summary>
    Task<(bool Success, string? ErrorMessage, int? GameId)> CreateGameChallengeAsync(string currentUserId, string opponentId);

    /// <summary>
    /// Carga el modelo para la Fase 1 (Posicionamiento de barcos) o la pantalla de espera.
    /// </summary>
    Task<(bool Success, string? ErrorMessage, BattleshipPlacementViewModel? ViewModel)> GetPlacementViewModelAsync(int gameId, string currentUserId);

    /// <summary>
    /// Valida y guarda la colocación de la flota de 5 barcos para el jugador.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> SavePlacementAsync(string currentUserId, int gameId, List<ShipPlacementDto> placements);

    /// <summary>
    /// Carga el tablero de combate para la Fase 2 (Ataques y radar) o estado final.
    /// </summary>
    Task<(bool Success, string? ErrorMessage, BattleshipPlayViewModel? ViewModel)> GetPlayViewModelAsync(int gameId, string currentUserId);

    /// <summary>
    /// Ejecuta un ataque a una coordenada (fila, columna) en el tablero rival.
    /// </summary>
    Task<BattleshipAttackResultDto> ExecuteAttackAsync(string currentUserId, int gameId, int row, int col);

    /// <summary>
    /// Permite al jugador rendirse y otorga la victoria al rival.
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> SurrenderGameAsync(string currentUserId, int gameId);

    /// <summary>
    /// Carga la vista de solo lectura del historial con los tableros finales de ambos jugadores.
    /// </summary>
    Task<(bool Success, string? ErrorMessage, BattleshipHistoryViewModel? ViewModel)> GetHistoryViewModelAsync(int gameId, string currentUserId);
}
