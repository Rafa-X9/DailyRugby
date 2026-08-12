using DailyRugby.Domain;

namespace DailyRugby.Application.Interfaces;

public interface IGameSimulatorFactory
{
    ISpecificGameSimulator GetGameSimulator(Seasons season);
}