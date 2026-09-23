using DailyRugby.Application.DTOs;
using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IGameSimulatorManager
{
    public event EventHandler GameEventHappened;

    Task<Result> ScheduleGameAsync(Guid gameId, DateTime dateTimeUtc);

    Task<IList<Schedule>> SeeScheduledGamesAsync(Guid champId, bool futureOnly = true);

    Result AddCheer(CheerAddRequest request);

    Result<IReadOnlyList<Player>> GetPlayersFromGame(Teams team);
}