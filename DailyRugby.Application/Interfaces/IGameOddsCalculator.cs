namespace DailyRugby.Application.Interfaces;

public interface IGameOddsCalculator
{
    Task CalculateAsync(Guid gameId);
}