using Discord;

namespace DailyRugby.Web.BotServices;

public class MemoryTester : BackgroundService
{
    private IMessageChannel _channel = null!;
    private readonly ulong _channelId;
    private readonly bool _isThread;
    private readonly TimeSpan _delay;

    public MemoryTester(IConfiguration configuration)
    {
        _channelId = ulong.Parse(configuration["DailyRugby:MemoryCheck:ChannelId"] ?? "");
        _isThread = bool.Parse(configuration["DailyRugby:MemoryCheck:IsThread"] ?? "");
        int minutes = int.Parse(configuration["DailyRugby:MemoryCheck:IntervalMinutes"] ?? "");
        _delay = TimeSpan.FromMinutes(minutes);
    }

    private async Task InitializeAsync()
    {
        var channel = await Program.DiscordClient.GetChannelAsync(_channelId);
        if (_isThread) _channel = (IThreadChannel)channel;
        else _channel = (IMessageChannel)channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeAsync();

        while (!stoppingToken.IsCancellationRequested)
        {

        }
    }
}