using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IGameSimulatorManager
{
    Task<Result> ScheduleGameAsync(Guid gameId, DateTime dateTimeUtc);

    Task<IList<Schedule>> SeeScheduledGamesAsync(Guid champId, bool futureOnly = true);
}