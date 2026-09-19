using DailyRugby.Application.Interfaces;

namespace DailyRugby.Application.Utilitaries;

public class SystemTimer : IGameTimer
{
    public async Task WaitUntilNextMinuteAsync()
    {
        var now = DateTime.UtcNow;
        TimeSpan untilNextMinute = TimeSpan.FromSeconds(60 - now.Second);
        await Task.Delay(untilNextMinute);
    }

    public async Task WaitFifteenSecondsAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(15));
    }

    public async Task WaitFifteenMinutesAsync()
    {
        await Task.Delay(TimeSpan.FromMinutes(15));
    }
}

public class SpedUpTimer : IGameTimer
{
    public async Task WaitUntilNextMinuteAsync()
    {
        var now = DateTime.UtcNow;
        var secondsUntil = 30 - now.Second % 30;
        var timeUntil = TimeSpan.FromSeconds(secondsUntil);
        await Task.Delay(timeUntil);
    }

    public async Task WaitFifteenSecondsAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(7.5));
    }

    public async Task WaitFifteenMinutesAsync()
    {
        await Task.Delay(TimeSpan.FromMinutes(2));
    }
}

public class InstantTimer : IGameTimer
{
    public Task WaitUntilNextMinuteAsync() => Task.CompletedTask;
    public Task WaitFifteenSecondsAsync() => Task.CompletedTask;
    public Task WaitFifteenMinutesAsync() => Task.CompletedTask;
}