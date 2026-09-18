using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface ISpecificGameSimulator
{
    GameEvent SimulateNextMinute(Game game);

    Task SaveGameAsync(GameEvent gameEvent, AppDbContext db);

    Result AddCheer(Cheer cheer, Game game);
}