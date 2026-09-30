using DailyRugby.Application.DTOs;
using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface ISpecificGameSimulator
{
    GameEvent SimulateNextMinute(Game game);

    Task SaveGameAsync(GameEvent gameEvent, AppDbContext db);

    Result<CheerResponse> AddCheer(Cheer cheer, Game game);
}