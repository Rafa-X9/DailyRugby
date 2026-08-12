using DailyRugby.Domain;

namespace DailyRugby.Application.Interfaces;

public interface ISpecificGameSimulator
{
    Task<GameEvent> SimulateNextMinute(Game game);
}