using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.Interfaces.Repositories;

public interface IBattleshipRepository : IGenericRepository<BattleshipGame>
{
    /// <summary>
    /// Obtiene una partida con todos sus detalles (jugadores, barcos con coordenadas y ataques con atacante).
    /// </summary>
    Task<BattleshipGame?> GetGameWithDetailsAsync(int gameId, bool disableTracking = false);

    /// <summary>
    /// Retorna todas las partidas activas (en fase Placement o InProgress) donde participe el usuario.
    /// </summary>
    Task<IReadOnlyList<BattleshipGame>> GetActiveGamesByUserAsync(string userId);

    /// <summary>
    /// Retorna el historial de partidas terminadas (Finished) donde haya participado el usuario.
    /// </summary>
    Task<IReadOnlyList<BattleshipGame>> GetFinishedGamesByUserAsync(string userId);

    /// <summary>
    /// Obtiene una partida activa entre dos usuarios si existe, para evitar retos duplicados.
    /// </summary>
    Task<BattleshipGame?> GetActiveGameBetweenUsersAsync(string userAId, string userBId);

    /// <summary>
    /// Verifica si dos usuarios tienen una partida activa en curso.
    /// </summary>
    Task<bool> HasActiveGameBetweenUsersAsync(string userAId, string userBId);

    /// <summary>
    /// Inserta los barcos y sus coordenadas en la base de datos.
    /// </summary>
    Task AddShipsAsync(IEnumerable<BattleshipShip> ships);

    /// <summary>
    /// Inserta un ataque realizado en la base de datos.
    /// </summary>
    Task AddAttackAsync(BattleshipAttack attack);
}
