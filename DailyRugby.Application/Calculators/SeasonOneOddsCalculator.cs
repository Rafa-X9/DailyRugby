using DailyRugby.Application.Interfaces;

namespace DailyRugby.Application.Calculators;

public class SeasonOneOddsCalculator(IServiceProvider serviceProvider)
    : IGameOddsCalculator
{
    public Task CalculateAsync(Guid gameId)
    {
        throw new NotImplementedException();
    }
}