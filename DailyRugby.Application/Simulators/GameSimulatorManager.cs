using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Simulators;

public class GameSimulatorManager : IGameSimulatorManager
{
    public Task<Result> ScheduleGame(Guid gameId, DateTime dateTimeUtc)
    {
        throw new NotImplementedException();
    }

    public Task<IList<Schedule>> SeeScheduledGames(Guid champId, bool futureOnly = true)
    {
        throw new NotImplementedException();
    }
}