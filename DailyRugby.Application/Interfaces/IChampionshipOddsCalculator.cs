using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IChampionshipOddsCalculator
{
    Task<Result<ChampionshipOdds>> GetOddsAsync(Guid champId, bool passIfNotExists = false);
}