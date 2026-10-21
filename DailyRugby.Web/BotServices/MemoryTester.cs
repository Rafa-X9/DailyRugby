using Discord;
using System.Diagnostics;
using System.Globalization;

namespace DailyRugby.Web.BotServices;

public class MemoryTester : BackgroundService
{
    private IMessageChannel _channel = null!;
    private readonly bool _isEnabled;
    private readonly ulong _channelId;
    private readonly bool _isThread;
    private readonly TimeSpan _delay;

    private long _highestMemory;
    private long _lowestMemory;

    public MemoryTester(IConfiguration configuration)
    {
        _isEnabled = bool.Parse(configuration["DailyRugby:MemoryCheck:IsEnabled"] ?? "");
        _channelId = ulong.Parse(configuration["DailyRugby:MemoryCheck:ChannelId"] ?? "");
        _isThread = bool.Parse(configuration["DailyRugby:MemoryCheck:IsThread"] ?? "");
        int minutes = int.Parse(configuration["DailyRugby:MemoryCheck:IntervalMinutes"] ?? "");
        _delay = TimeSpan.FromMinutes(minutes);

        _highestMemory = Process.GetCurrentProcess().PrivateMemorySize64;
        _lowestMemory = _highestMemory;
    }

    private async Task InitializeAsync()
    {
        var channel = await Program.DiscordClient.GetChannelAsync(_channelId);
        if (_isThread) _channel = (IThreadChannel)channel;
        else _channel = (IMessageChannel)channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_isEnabled) return;

        await InitializeAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_delay, stoppingToken);

            long memoryUsage = Process.GetCurrentProcess().PrivateMemorySize64;
            if (memoryUsage > _highestMemory) _highestMemory = memoryUsage;
            else if (memoryUsage < _lowestMemory) _lowestMemory = memoryUsage;

            CultureInfo ci = CultureInfo.InvariantCulture;
            string message = $"Currently using {(memoryUsage / 1_000_000.0).ToString("F2", ci)} " +
                $"megabytes of RAM. Highest is {(_highestMemory / 1_000_000.0).ToString("F2", ci)} " +
                $"while lowest is {(_lowestMemory / 1_000_000.0).ToString("F2", ci)}.";

            await _channel.SendMessageAsync(message);
        }
    }
}