using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IGameSimulatorManager
{
    Task<Result> ScheduleGame(Guid gameId, DateTime dateTimeUtc);
}