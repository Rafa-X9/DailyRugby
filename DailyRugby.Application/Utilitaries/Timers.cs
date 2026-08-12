using DailyRugby.Application.Interfaces;

namespace DailyRugby.Application.Utilitaries;

public class SystemTimer : IGameTimer
{
    public async Task WaitOneMinuteAsync()
    {
        await Task.Delay(TimeSpan.FromMinutes(1));
    }

    public async Task WaitFifteenMinutesAsync()
    {
        await Task.Delay(TimeSpan.FromMinutes(15));
    }
}

public class SpedUpTimer : IGameTimer
{
    public async Task WaitOneMinuteAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(15));
    }

    public async Task WaitFifteenMinutesAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(15 * 15));
    }
}

public class InstantTimer : IGameTimer
{
    public Task WaitOneMinuteAsync() => Task.CompletedTask;
    public Task WaitFifteenMinutesAsync() => Task.CompletedTask;
}