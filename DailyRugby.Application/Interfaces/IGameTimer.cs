namespace DailyRugby.Application.Interfaces;

public interface IGameTimer
{
    Task WaitOneMinuteAsync();
    Task WaitFifteenMinutesAsync();
}