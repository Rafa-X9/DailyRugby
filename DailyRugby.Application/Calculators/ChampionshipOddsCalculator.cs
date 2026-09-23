using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Calculators;

public class ChampionshipOddsCalculator : IChampionshipOddsCalculator
{
    public async Task<Result<ChampionshipOdds>> GetOddsAsync(Guid champId, bool passIfNotExists = false)
    {
        throw new NotImplementedException();
    }
}