using DailyRugby.Application.DTOs;
using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IGameCrudService
{
    Task<Result<IList<GameResponse>>> GenerateRounds(Guid champId, bool overwriteIfExists = false);

    Task<IList<GameResponse>> GetAllAsync();

    Task<IList<GameResponse>> GetAllAsync(Guid champId);

    Task<Result<IList<GameResponse>>> GetCurrentRoundAsync();

    Task<Result<IList<GameResponse>>> GetRoundAsync(Guid champId, int round);

    Task<Result<IList<GameResponse>>> GetByTeamIdAsync(Guid teamId);

    Task<Result<TeamGameResponse>> SetTacticAsync(Guid gameId, Teams team, Tactics tactic);
}