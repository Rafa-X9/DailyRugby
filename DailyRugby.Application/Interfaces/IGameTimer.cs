namespace DailyRugby.Application.Interfaces;

public interface IGameTimer
{
    Task WaitUntilNextMinuteAsync();
    Task WaitFifteenSecondsAsync();
    Task WaitFifteenMinutesAsync();
}