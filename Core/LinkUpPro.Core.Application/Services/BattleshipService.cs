using System.Text.Json;
using LinkUpPro.Core.Application.Interfaces.Repositories;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModels.Battleship;
using LinkUpPro.Core.Application.ViewModels.Friend;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enums;

namespace LinkUpPro.Core.Application.Services;

public class BattleshipService : IBattleshipService
{
    private const int BoardSize = 12;
    private readonly IBattleshipRepository _battleshipRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IGenericRepository<Notification> _notificationRepository;

    public BattleshipService(
        IBattleshipRepository battleshipRepository,
        IFriendshipRepository friendshipRepository,
        IGenericRepository<Notification> notificationRepository)
    {
        _battleshipRepository = battleshipRepository;
        _friendshipRepository = friendshipRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<BattleshipIndexViewModel> GetIndexViewModelAsync(string currentUserId)
    {
        var activeGames = await _battleshipRepository.GetActiveGamesByUserAsync(currentUserId);
        var finishedGames = await _battleshipRepository.GetFinishedGamesByUserAsync(currentUserId);

        // 1. Revisar y procesar regla de abandono por inactividad (+48 horas) en partidas activas
        var updatedActive = new List<BattleshipGame>();
        var updatedFinished = finishedGames.ToList();

        foreach (var game in activeGames)
        {
            if (await CheckAndHandleTimeoutAsync(game))
            {
                updatedFinished.Insert(0, game);
            }
            else
            {
                updatedActive.Add(game);
            }
        }

        // 2. Mapear partidas activas
        var activeViewModels = updatedActive.Select(g => MapToGameItemViewModel(g, currentUserId)).ToList();

        // 3. Mapear historial de partidas finalizadas
        var finishedViewModels = updatedFinished.Select(g => MapToGameItemViewModel(g, currentUserId)).ToList();

        // 4. Obtener amigos activos con los que no se tenga una partida en curso actualmente
        var activeFriends = await _friendshipRepository.GetActiveFriendsAsync(currentUserId);
        var busyFriendIds = updatedActive
            .Select(g => g.Player1Id == currentUserId ? g.Player2Id : g.Player1Id)
            .ToHashSet();

        var availableFriends = activeFriends
            .Where(f => !busyFriendIds.Contains(f.Id))
            .Select(f => new FriendViewModel
            {
                UserId = f.Id,
                FullName = f.FullName,
                UserName = f.UserName ?? string.Empty,
                ProfilePictureUrl = f.ProfilePictureUrl
            })
            .ToList();

        return new BattleshipIndexViewModel
        {
            ActiveGames = activeViewModels,
            FinishedGames = finishedViewModels,
            AvailableFriends = availableFriends
        };
    }

    public async Task<(bool Success, string? ErrorMessage, int? GameId)> CreateGameChallengeAsync(string currentUserId, string opponentId)
    {
        if (string.Equals(currentUserId, opponentId, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "No puedes retarte a ti mismo.", null);
        }

        var friendship = await _friendshipRepository.GetFriendshipBetweenUsersAsync(currentUserId, opponentId);
        if (friendship == null || friendship.Status != FriendshipStatus.Active)
        {
            return (false, "Solo puedes retar a usuarios que sean tus amigos activos.", null);
        }

        var hasActiveGame = await _battleshipRepository.HasActiveGameBetweenUsersAsync(currentUserId, opponentId);
        if (hasActiveGame)
        {
            return (false, "Ya tienes una partida activa con este amigo.", null);
        }

        var game = new BattleshipGame
        {
            Player1Id = currentUserId,
            Player2Id = opponentId,
            Status = GameStatus.Placement,
            Player1Ready = false,
            Player2Ready = false,
            CreatedAt = DateTime.UtcNow
        };

        var createdGame = await _battleshipRepository.AddAsync(game);

        // Notificar al amigo sobre el reto
        await _notificationRepository.AddAsync(new Notification
        {
            RecipientId = opponentId,
            ActorId = currentUserId,
            Type = NotificationType.BattleshipTurn,
            Message = "te ha retado a una partida de Battleship.",
            TargetUrl = $"/Battleship/Placement/{createdGame.Id}",
            CreatedAt = DateTime.UtcNow
        });

        return (true, null, createdGame.Id);
    }

    public async Task<(bool Success, string? ErrorMessage, BattleshipPlacementViewModel? ViewModel)> GetPlacementViewModelAsync(int gameId, string currentUserId)
    {
        var game = await _battleshipRepository.GetGameWithDetailsAsync(gameId, disableTracking: true);
        if (game == null)
        {
            return (false, "La partida especificada no existe.", null);
        }

        if (game.Player1Id != currentUserId && game.Player2Id != currentUserId)
        {
            return (false, "No tienes autorización para acceder a esta partida.", null);
        }

        var isPlayer1 = game.Player1Id == currentUserId;
        var opponent = isPlayer1 ? game.Player2 : game.Player1;
        var isReady = isPlayer1 ? game.Player1Ready : game.Player2Ready;
        var opponentReady = isPlayer1 ? game.Player2Ready : game.Player1Ready;

        var placedShips = game.Ships
            .Where(s => s.PlayerId == currentUserId)
            .Select(s => new ShipPlacementDto
            {
                ShipType = s.ShipType,
                Size = s.Size,
                StartRow = s.StartRow,
                StartCol = s.StartCol,
                Direction = s.Direction
            })
            .ToList();

        var viewModel = new BattleshipPlacementViewModel
        {
            GameId = game.Id,
            OpponentFullName = opponent.FullName,
            OpponentUserName = opponent.UserName ?? string.Empty,
            OpponentProfilePictureUrl = opponent.ProfilePictureUrl,
            IsReady = isReady,
            OpponentReady = opponentReady,
            BoardSize = BoardSize,
            PlacedShips = placedShips
        };

        return (true, null, viewModel);
    }

    public async Task<(bool Success, string? ErrorMessage)> SavePlacementAsync(string currentUserId, int gameId, List<ShipPlacementDto> placements)
    {
        var game = await _battleshipRepository.GetGameWithDetailsAsync(gameId, disableTracking: false);
        if (game == null)
        {
            return (false, "La partida no existe.");
        }

        if (game.Player1Id != currentUserId && game.Player2Id != currentUserId)
        {
            return (false, "No tienes permiso para interactuar con esta partida.");
        }

        if (game.Status != GameStatus.Placement)
        {
            return (false, "La partida ya no se encuentra en fase de colocación.");
        }

        var isPlayer1 = game.Player1Id == currentUserId;
        var alreadyReady = isPlayer1 ? game.Player1Ready : game.Player2Ready;
        if (alreadyReady)
        {
            return (false, "Ya has confirmado la colocación de tu flota anteriormente.");
        }

        // Validación de los 5 barcos reglamentarios (2, 3, 3, 4, 5)
        var validationResult = ValidateFleetPlacement(placements);
        if (!validationResult.IsValid)
        {
            return (false, validationResult.ErrorMessage);
        }

        // Generar entidades de barcos y sus coordenadas
        var shipsToAdd = new List<BattleshipShip>();

        foreach (var p in placements)
        {
            var ship = new BattleshipShip
            {
                GameId = game.Id,
                PlayerId = currentUserId,
                ShipType = p.ShipType,
                Size = p.Size,
                StartRow = p.StartRow,
                StartCol = p.StartCol,
                Direction = p.Direction,
                IsSunk = false
            };

            var coordinates = CalculateCoordinates(p.StartRow, p.StartCol, p.Direction, p.Size);
            foreach (var (row, col) in coordinates)
            {
                ship.Coordinates.Add(new BattleshipShipCoordinate
                {
                    Row = row,
                    Col = col,
                    IsHit = false
                });
            }

            shipsToAdd.Add(ship);
        }

        await _battleshipRepository.AddShipsAsync(shipsToAdd);

        // Actualizar estado de preparación del jugador
        if (isPlayer1)
        {
            game.Player1Ready = true;
        }
        else
        {
            game.Player2Ready = true;
        }

        // Si ambos completaron la colocación, iniciamos la partida (Fase 2)
        if (game.Player1Ready && game.Player2Ready)
        {
            game.Status = GameStatus.InProgress;
            game.StartedAt = DateTime.UtcNow;
            game.CurrentTurnPlayerId = game.Player1Id; // Regla: Jugador 1 ataca primero
            game.LastMoveAt = DateTime.UtcNow;

            // Notificar a ambos que el juego ha comenzado
            await _notificationRepository.AddAsync(new Notification
            {
                RecipientId = game.Player1Id,
                ActorId = game.Player2Id,
                Type = NotificationType.BattleshipTurn,
                Message = "¡Ambos jugadores listos! La partida de Battleship ha comenzado y es tu turno de atacar.",
                TargetUrl = $"/Battleship/Play/{game.Id}",
                CreatedAt = DateTime.UtcNow
            });

            await _notificationRepository.AddAsync(new Notification
            {
                RecipientId = game.Player2Id,
                ActorId = game.Player1Id,
                Type = NotificationType.BattleshipTurn,
                Message = "¡Ambos listos! La partida ha comenzado. Esperando el ataque del creador.",
                TargetUrl = $"/Battleship/Play/{game.Id}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _battleshipRepository.UpdateAsync(game);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage, BattleshipPlayViewModel? ViewModel)> GetPlayViewModelAsync(int gameId, string currentUserId)
    {
        var game = await _battleshipRepository.GetGameWithDetailsAsync(gameId, disableTracking: false);
        if (game == null)
        {
            return (false, "Partida no encontrada.", null);
        }

        if (game.Player1Id != currentUserId && game.Player2Id != currentUserId)
        {
            return (false, "No tienes acceso a esta partida.", null);
        }

        // Verificar abandono por inactividad de 48 horas
        await CheckAndHandleTimeoutAsync(game);

        var isPlayer1 = game.Player1Id == currentUserId;
        var opponent = isPlayer1 ? game.Player2 : game.Player1;
        var opponentId = opponent.Id;

        // Horas restantes para abandono
        double? hoursLeft = null;
        if (game.Status == GameStatus.InProgress && game.LastMoveAt.HasValue)
        {
            var elapsed = DateTime.UtcNow - game.LastMoveAt.Value;
            hoursLeft = Math.Max(0, Math.Round(48 - elapsed.TotalHours, 1));
        }

        // Construir Tablero Propio (12x12)
        var myBoard = BuildEmptyGrid();
        var myShips = game.Ships.Where(s => s.PlayerId == currentUserId).ToList();

        foreach (var ship in myShips)
        {
            foreach (var coord in ship.Coordinates)
            {
                if (coord.Row >= 0 && coord.Row < BoardSize && coord.Col >= 0 && coord.Col < BoardSize)
                {
                    var cell = myBoard[coord.Row][coord.Col];
                    cell.HasShip = true;
                    cell.ShipType = ship.ShipType;
                    cell.IsAttacked = coord.IsHit;
                    cell.IsHit = coord.IsHit;
                }
            }
        }

        // Ataques fallidos del oponente sobre mi tablero (agua)
        var opponentAttacks = game.Attacks.Where(a => a.AttackerId == opponentId).ToList();
        foreach (var attack in opponentAttacks)
        {
            if (attack.Row >= 0 && attack.Row < BoardSize && attack.Col >= 0 && attack.Col < BoardSize)
            {
                var cell = myBoard[attack.Row][attack.Col];
                cell.IsAttacked = true;
                cell.IsHit = attack.IsHit;
            }
        }

        // Construir Tablero de Radar/Ataque (12x12): disparos que yo he realizado
        var radarBoard = BuildEmptyGrid();
        var myAttacks = game.Attacks.Where(a => a.AttackerId == currentUserId).ToList();
        foreach (var attack in myAttacks)
        {
            if (attack.Row >= 0 && attack.Row < BoardSize && attack.Col >= 0 && attack.Col < BoardSize)
            {
                var cell = radarBoard[attack.Row][attack.Col];
                cell.IsAttacked = true;
                cell.IsHit = attack.IsHit;
            }
        }

        // Estado detallado de las flotas
        var myShipDetails = myShips.Select(s => new BattleshipShipDetailViewModel
        {
            ShipType = s.ShipType,
            Name = GetShipDisplayName(s.ShipType),
            Size = s.Size,
            IsSunk = s.IsSunk,
            HitsCount = s.Coordinates.Count(c => c.IsHit)
        }).ToList();

        var opponentShips = game.Ships.Where(s => s.PlayerId == opponentId).ToList();
        var opponentShipDetails = opponentShips.Select(s => new BattleshipShipDetailViewModel
        {
            ShipType = s.ShipType,
            Name = GetShipDisplayName(s.ShipType),
            Size = s.Size,
            IsSunk = s.IsSunk,
            HitsCount = s.Coordinates.Count(c => c.IsHit)
        }).ToList();

        var isMyTurn = game.Status == GameStatus.InProgress && game.CurrentTurnPlayerId == currentUserId;
        var currentTurnUser = game.CurrentTurnPlayerId == game.Player1Id ? game.Player1 : game.Player2;

        var viewModel = new BattleshipPlayViewModel
        {
            GameId = game.Id,
            Status = game.Status,
            CurrentUserId = currentUserId,
            Player1Id = game.Player1Id,
            Player2Id = game.Player2Id,
            OpponentId = opponentId,
            OpponentFullName = opponent.FullName,
            OpponentUserName = opponent.UserName ?? string.Empty,
            OpponentProfilePictureUrl = opponent.ProfilePictureUrl,
            IsMyTurn = isMyTurn,
            CurrentTurnUserName = currentTurnUser?.FullName ?? string.Empty,
            LastMoveAt = game.LastMoveAt,
            RemainingHoursToTimeout = hoursLeft,
            WinnerId = game.WinnerPlayerId,
            WinnerName = game.WinnerPlayer?.FullName,
            DidIWin = game.WinnerPlayerId == currentUserId,
            EndReason = game.EndReason,
            BoardSize = BoardSize,
            MyBoard = myBoard,
            RadarBoard = radarBoard,
            MyShips = myShipDetails,
            OpponentShips = opponentShipDetails
        };

        return (true, null, viewModel);
    }

    public async Task<BattleshipAttackResultDto> ExecuteAttackAsync(string currentUserId, int gameId, int row, int col)
    {
        var game = await _battleshipRepository.GetGameWithDetailsAsync(gameId, disableTracking: false);
        if (game == null)
        {
            return new BattleshipAttackResultDto { Success = false, Message = "Partida no encontrada." };
        }

        if (game.Player1Id != currentUserId && game.Player2Id != currentUserId)
        {
            return new BattleshipAttackResultDto { Success = false, Message = "No perteneces a esta partida." };
        }

        // Revisar inactividad de 48h antes de permitir el ataque
        if (await CheckAndHandleTimeoutAsync(game))
        {
            return new BattleshipAttackResultDto
            {
                Success = false,
                Message = "La partida finalizó debido al límite de inactividad de 48 horas.",
                IsGameOver = true,
                WinnerName = game.WinnerPlayer?.FullName
            };
        }

        if (game.Status != GameStatus.InProgress)
        {
            return new BattleshipAttackResultDto { Success = false, Message = "La partida no se encuentra en curso." };
        }

        if (game.CurrentTurnPlayerId != currentUserId)
        {
            return new BattleshipAttackResultDto { Success = false, Message = "No es tu turno de atacar. Espera el disparo de tu rival." };
        }

        if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
        {
            return new BattleshipAttackResultDto { Success = false, Message = $"Las coordenadas deben estar entre 0 y {BoardSize - 1}." };
        }

        // Regla: no permitir repetir celdas atacadas
        var alreadyAttacked = game.Attacks.Any(a => a.AttackerId == currentUserId && a.Row == row && a.Col == col);
        if (alreadyAttacked)
        {
            return new BattleshipAttackResultDto { Success = false, Message = "Ya has atacado esta casilla anteriormente." };
        }

        var opponentId = game.Player1Id == currentUserId ? game.Player2Id : game.Player1Id;
        var opponentShips = game.Ships.Where(s => s.PlayerId == opponentId).ToList();

        // Buscar si alguna coordenada de barco del rival coincide con el disparo
        BattleshipShipCoordinate? hitCoordinate = null;
        BattleshipShip? hitShip = null;

        foreach (var ship in opponentShips)
        {
            var match = ship.Coordinates.FirstOrDefault(c => c.Row == row && c.Col == col);
            if (match != null)
            {
                hitCoordinate = match;
                hitShip = ship;
                break;
            }
        }

        var isHit = hitCoordinate != null;
        var isSunk = false;
        string? sunkShipName = null;
        var isGameOver = false;

        if (isHit && hitCoordinate != null && hitShip != null)
        {
            hitCoordinate.IsHit = true;

            // Evaluar si todo el barco fue destruido
            if (hitShip.Coordinates.All(c => c.IsHit))
            {
                hitShip.IsSunk = true;
                isSunk = true;
                sunkShipName = GetShipDisplayName(hitShip.ShipType);

                // Evaluar si todos los barcos del rival fueron destruidos (Victoria)
                if (opponentShips.All(s => s.IsSunk || s.Coordinates.All(c => c.IsHit)))
                {
                    isGameOver = true;
                    game.Status = GameStatus.Finished;
                    game.WinnerPlayerId = currentUserId;
                    game.EndReason = GameEndReason.AllShipsSunk;
                    game.FinishedAt = DateTime.UtcNow;

                    await _notificationRepository.AddAsync(new Notification
                    {
                        RecipientId = opponentId,
                        ActorId = currentUserId,
                        Type = NotificationType.BattleshipGameOver,
                        Message = "ha hundido toda tu flota y ha ganado la partida de Battleship.",
                        TargetUrl = $"/Battleship/HistoryDetails/{game.Id}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Alternancia estricta de turnos si el juego no terminó
        if (!isGameOver)
        {
            game.CurrentTurnPlayerId = opponentId;
            game.LastMoveAt = DateTime.UtcNow;

            await _notificationRepository.AddAsync(new Notification
            {
                RecipientId = opponentId,
                ActorId = currentUserId,
                Type = NotificationType.BattleshipTurn,
                Message = isHit
                    ? (isSunk ? $"ha hundido tu {sunkShipName}! Es tu turno de disparar." : "ha impactado uno de tus barcos! Es tu turno de disparar.")
                    : "ha disparado al agua. ¡Es tu turno de atacar!",
                TargetUrl = $"/Battleship/Play/{game.Id}",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Registrar el ataque
        var attack = new BattleshipAttack
        {
            GameId = game.Id,
            AttackerId = currentUserId,
            Row = row,
            Col = col,
            IsHit = isHit,
            CreatedAt = DateTime.UtcNow
        };

        await _battleshipRepository.AddAttackAsync(attack);
        await _battleshipRepository.UpdateAsync(game);

        return new BattleshipAttackResultDto
        {
            Success = true,
            Message = isGameOver
                ? "¡Impacto crítico! Has hundido el último barco y ganado la partida."
                : (isSunk
                    ? $"¡Acierto! Has hundido el {sunkShipName} enemigo."
                    : (isHit ? "¡Acierto! Has impactado un barco enemigo." : "Agua. Disparo fallido.")),
            IsHit = isHit,
            IsSunk = isSunk,
            SunkShipName = sunkShipName,
            IsGameOver = isGameOver,
            WinnerName = isGameOver ? (game.Player1Id == currentUserId ? game.Player1?.FullName : game.Player2?.FullName) : null
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> SurrenderGameAsync(string currentUserId, int gameId)
    {
        var game = await _battleshipRepository.GetGameWithDetailsAsync(gameId, disableTracking: false);
        if (game == null)
        {
            return (false, "Partida no encontrada.");
        }

        if (game.Player1Id != currentUserId && game.Player2Id != currentUserId)
        {
            return (false, "No tienes autorización para acceder a esta partida.");
        }

        if (game.Status == GameStatus.Finished)
        {
            return (false, "La partida ya ha finalizado.");
        }

        var opponentId = game.Player1Id == currentUserId ? game.Player2Id : game.Player1Id;

        game.Status = GameStatus.Finished;
        game.WinnerPlayerId = opponentId;
        game.EndReason = GameEndReason.Surrendered;
        game.FinishedAt = DateTime.UtcNow;

        await _battleshipRepository.UpdateAsync(game);

        // Notificar al ganador
        await _notificationRepository.AddAsync(new Notification
        {
            RecipientId = opponentId,
            ActorId = currentUserId,
            Type = NotificationType.BattleshipGameOver,
            Message = "se ha rendido. ¡Has ganado la partida de Battleship!",
            TargetUrl = $"/Battleship/HistoryDetails/{game.Id}",
            CreatedAt = DateTime.UtcNow
        });

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage, BattleshipHistoryViewModel? ViewModel)> GetHistoryViewModelAsync(int gameId, string currentUserId)
    {
        var game = await _battleshipRepository.GetGameWithDetailsAsync(gameId, disableTracking: true);
        if (game == null)
        {
            return (false, "Partida no encontrada.", null);
        }

        if (game.Player1Id != currentUserId && game.Player2Id != currentUserId)
        {
            return (false, "No tienes permiso para consultar esta partida.", null);
        }

        var isPlayer1 = game.Player1Id == currentUserId;
        var opponent = isPlayer1 ? game.Player2 : game.Player1;
        var opponentId = opponent.Id;

        // Tablero de colocación de barcos del usuario con los impactos recibidos
        var myFleetBoard = BuildEmptyGrid();
        var myShips = game.Ships.Where(s => s.PlayerId == currentUserId).ToList();

        foreach (var ship in myShips)
        {
            foreach (var coord in ship.Coordinates)
            {
                if (coord.Row >= 0 && coord.Row < BoardSize && coord.Col >= 0 && coord.Col < BoardSize)
                {
                    var cell = myFleetBoard[coord.Row][coord.Col];
                    cell.HasShip = true;
                    cell.ShipType = ship.ShipType;
                    cell.IsAttacked = coord.IsHit;
                    cell.IsHit = coord.IsHit;
                }
            }
        }

        // Tablero de ataques realizados por el usuario
        var myAttacksBoard = BuildEmptyGrid();
        foreach (var attack in game.Attacks.Where(a => a.AttackerId == currentUserId))
        {
            if (attack.Row >= 0 && attack.Row < BoardSize && attack.Col >= 0 && attack.Col < BoardSize)
            {
                var cell = myAttacksBoard[attack.Row][attack.Col];
                cell.IsAttacked = true;
                cell.IsHit = attack.IsHit;
            }
        }

        // Tablero de ataques realizados por el rival
        var opponentAttacksBoard = BuildEmptyGrid();
        foreach (var attack in game.Attacks.Where(a => a.AttackerId == opponentId))
        {
            if (attack.Row >= 0 && attack.Row < BoardSize && attack.Col >= 0 && attack.Col < BoardSize)
            {
                var cell = opponentAttacksBoard[attack.Row][attack.Col];
                cell.IsAttacked = true;
                cell.IsHit = attack.IsHit;
            }
        }

        var viewModel = new BattleshipHistoryViewModel
        {
            GameId = game.Id,
            CurrentUserId = currentUserId,
            Player1Name = game.Player1.FullName,
            Player2Name = game.Player2.FullName,
            OpponentName = opponent.FullName,
            WinnerName = game.WinnerPlayer?.FullName,
            DidIWin = game.WinnerPlayerId == currentUserId,
            EndReason = game.EndReason,
            CreatedAt = game.CreatedAt,
            FinishedAt = game.FinishedAt,
            BoardSize = BoardSize,
            MyFleetBoard = myFleetBoard,
            MyAttacksBoard = myAttacksBoard,
            OpponentAttacksBoard = opponentAttacksBoard
        };

        return (true, null, viewModel);
    }

    #region Reglas Privadas y Validaciones de Battleship

    private async Task<bool> CheckAndHandleTimeoutAsync(BattleshipGame game)
    {
        if (game.Status == GameStatus.InProgress && game.LastMoveAt.HasValue)
        {
            var elapsed = DateTime.UtcNow - game.LastMoveAt.Value;
            if (elapsed >= TimeSpan.FromHours(48))
            {
                var timedOutPlayerId = game.CurrentTurnPlayerId;
                var winnerId = timedOutPlayerId == game.Player1Id ? game.Player2Id : game.Player1Id;

                game.Status = GameStatus.Finished;
                game.WinnerPlayerId = winnerId;
                game.EndReason = GameEndReason.TimedOut48h;
                game.FinishedAt = DateTime.UtcNow;

                await _battleshipRepository.UpdateAsync(game);

                await _notificationRepository.AddAsync(new Notification
                {
                    RecipientId = winnerId,
                    ActorId = timedOutPlayerId ?? string.Empty,
                    Type = NotificationType.BattleshipGameOver,
                    Message = "ha sido descalificado por superar las 48 horas de inactividad. ¡Has ganado la partida!",
                    TargetUrl = $"/Battleship/HistoryDetails/{game.Id}",
                    CreatedAt = DateTime.UtcNow
                });

                return true;
            }
        }

        return false;
    }

    private static (bool IsValid, string? ErrorMessage) ValidateFleetPlacement(List<ShipPlacementDto> placements)
    {
        if (placements == null || placements.Count != 5)
        {
            return (false, "Debes posicionar exactamente los 5 barcos reglamentarios.");
        }

        // Comprobar que estén los tamaños requeridos: 5, 4, 3, 3, 2
        var requiredSizes = new List<int> { 5, 4, 3, 3, 2 };
        var submittedSizes = placements.Select(p => p.Size).OrderByDescending(s => s).ToList();

        if (!submittedSizes.SequenceEqual(requiredSizes))
        {
            return (false, "La configuración de barcos debe contener exactamente: 1 de tamaño 5, 1 de tamaño 4, 2 de tamaño 3 y 1 de tamaño 2.");
        }

        var occupiedCoordinates = new HashSet<(int Row, int Col)>();

        foreach (var ship in placements)
        {
            var coords = CalculateCoordinates(ship.StartRow, ship.StartCol, ship.Direction, ship.Size);

            foreach (var (r, c) in coords)
            {
                // Validación de límites del tablero 12x12
                if (r < 0 || r >= BoardSize || c < 0 || c >= BoardSize)
                {
                    return (false, $"El barco {GetShipDisplayName(ship.ShipType)} se sale de los límites del tablero (12x12).");
                }

                // Validación de superposición
                if (!occupiedCoordinates.Add((r, c)))
                {
                    return (false, $"Superposición detectada en la casilla ({r + 1}, {c + 1}). Los barcos no pueden cruzarse.");
                }
            }
        }

        return (true, null);
    }

    private static List<(int Row, int Col)> CalculateCoordinates(int startRow, int startCol, ShipDirection direction, int size)
    {
        var coordinates = new List<(int Row, int Col)>();

        int deltaRow = 0;
        int deltaCol = 0;

        switch (direction)
        {
            case ShipDirection.Up: deltaRow = -1; break;
            case ShipDirection.Down: deltaRow = 1; break;
            case ShipDirection.Left: deltaCol = -1; break;
            case ShipDirection.Right: deltaCol = 1; break;
        }

        for (int i = 0; i < size; i++)
        {
            coordinates.Add((startRow + (i * deltaRow), startCol + (i * deltaCol)));
        }

        return coordinates;
    }

    private static List<List<BoardCellViewModel>> BuildEmptyGrid()
    {
        var grid = new List<List<BoardCellViewModel>>();

        for (int r = 0; r < BoardSize; r++)
        {
            var row = new List<BoardCellViewModel>();
            for (int c = 0; c < BoardSize; c++)
            {
                row.Add(new BoardCellViewModel
                {
                    Row = r,
                    Col = c,
                    HasShip = false,
                    IsAttacked = false,
                    IsHit = false
                });
            }
            grid.Add(row);
        }

        return grid;
    }

    private static BattleshipGameItemViewModel MapToGameItemViewModel(BattleshipGame game, string currentUserId)
    {
        var isPlayer1 = game.Player1Id == currentUserId;
        var opponent = isPlayer1 ? game.Player2 : game.Player1;
        var isReady = isPlayer1 ? game.Player1Ready : game.Player2Ready;

        return new BattleshipGameItemViewModel
        {
            GameId = game.Id,
            OpponentId = opponent.Id,
            OpponentFullName = opponent.FullName,
            OpponentUserName = opponent.UserName ?? string.Empty,
            OpponentProfilePictureUrl = opponent.ProfilePictureUrl,
            Status = game.Status,
            IsMyTurn = game.Status == GameStatus.InProgress && game.CurrentTurnPlayerId == currentUserId,
            IsPlayer1 = isPlayer1,
            Player1Ready = game.Player1Ready,
            Player2Ready = game.Player2Ready,
            IsReady = isReady,
            CreatedAt = game.CreatedAt,
            LastMoveAt = game.LastMoveAt,
            WinnerId = game.WinnerPlayerId,
            WinnerName = game.WinnerPlayer?.FullName,
            DidIWin = game.WinnerPlayerId == currentUserId,
            EndReason = game.EndReason,
            FinishedAt = game.FinishedAt
        };
    }

    private static string GetShipDisplayName(ShipType type) => type switch
    {
        ShipType.Carrier => "Portaaviones (5 casillas)",
        ShipType.Battleship => "Acorazado (4 casillas)",
        ShipType.Destroyer => "Destructor (3 casillas)",
        ShipType.Submarine => "Submarino (3 casillas)",
        ShipType.PatrolBoat => "Patrullero (2 casillas)",
        _ => "Barco"
    };

    #endregion
}
