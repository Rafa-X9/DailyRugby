using DailyRugby.Domain;

namespace DailyRugby.Application.Interfaces;

public interface ISpecificGameSimulator
{
    GameEvent SimulateNextMinute(Game game);
}