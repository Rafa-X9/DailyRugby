using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;

namespace DailyRugby.Application.Simulators;

public class SeasonThreeGameSimulator : ISpecificGameSimulator
{
    public Task SaveGameAsync(GameEvent gameEvent, AppDbContext db)
    {
        throw new NotImplementedException();
    }

    public GameEvent SimulateNextMinute(Game game)
    {
        throw new NotImplementedException();
    }
}