using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IGameOddsCalculator
{
    Task<Result<GameOdds>> GetOddsAsync(Guid gameId, bool passIfNotExists = false);
}