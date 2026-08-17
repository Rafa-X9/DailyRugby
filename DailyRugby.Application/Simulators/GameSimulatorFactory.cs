using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;

namespace DailyRugby.Application.Simulators;

public class GameSimulatorFactory : IGameSimulatorFactory
{
    public ISpecificGameSimulator GetGameSimulator(Seasons season)
    {
        return season switch
        {
            Seasons.Season1 => new SeasonOneGameSimulator(),
            _ => throw new NotImplementedException($"{season}'s game simulator not yet implemented")
        };
    }
}