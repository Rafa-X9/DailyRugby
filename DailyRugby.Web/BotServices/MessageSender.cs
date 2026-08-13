using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using Discord;

namespace DailyRugby.Web.BotServices;

public class MessageSender
{
    private readonly IConfiguration _configuration;
    private readonly ulong _channelId;

    public MessageSender(IGameSimulatorManager simulator, IConfiguration configuration)
    {
        simulator.GameEventHappened += OnGameEventHappened;
        _configuration = configuration;
        _channelId = ulong.Parse(_configuration["ChannelId"] ?? throw new Exception());
    }

    private async void OnGameEventHappened(object? sender, EventArgs e)
    {
        var gameEvent = (GameEvent)e;
        var channel = (IMessageChannel)await Program.DiscordClient.GetChannelAsync(_channelId);
        
        switch (gameEvent.EventType)
        {
            case GameEventType.GameStarted:
                await AnnounceGameStartAsync(channel, gameEvent);
                break;
            case GameEventType.GameFinished:
                await AnnounceGameEndAsync(channel, gameEvent);
                break;
            default:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - Something happened!!!! " +
                    $"{gameEvent.EventType}");
                break;
        }
    }

    private async Task AnnounceGameStartAsync(IMessageChannel channel, GameEvent gameEvent)
    {
        await channel.SendMessageAsync($"The game between {gameEvent.Game.Teams[0].Team.Country} and " +
            $"{gameEvent.Game.Teams[1].Team.Country} begins!");
    }

    private async Task AnnounceGameEndAsync(IMessageChannel channel, GameEvent gameEvent)
    {
        await channel.SendMessageAsync($"Game over! Final score: " +
            $"{gameEvent.Game.Teams[0].Team.Country} {gameEvent.TeamAScore} x " +
            $"{gameEvent.Game.Teams[1].Team.Country} {gameEvent.TeamBScore}");
    }
}