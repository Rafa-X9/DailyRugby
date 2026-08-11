using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using Discord;

namespace DailyRugby.Web.BotServices;

public class MessageSender
{
    private readonly IConfiguration _configuration;

    public MessageSender(IGameSimulatorManager simulator, IConfiguration configuration)
    {
        simulator.GameEventHappened += OnGameEventHappened;
        _configuration = configuration;
    }

    private async void OnGameEventHappened(object? sender, EventArgs e)
    {
        var gameEvent = (GameEvent)e;
        ulong channelId = ulong.Parse(_configuration["ChannelId"] ?? throw new Exception());
        var channel = (IMessageChannel)await Program.DiscordClient.GetChannelAsync(channelId);
        await channel.SendMessageAsync("Some game started woah");
    }
}