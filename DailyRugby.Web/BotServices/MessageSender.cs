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

            case GameEventType.HalfTime:
                await AnnounceIntervalAsync(channel,
                    gameEvent,
                    gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country);
                break;

            //------------------------------

            case GameEventType.TeamAFailedTry:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + TryAttempt(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync($"{FailedTry(gameEvent.Game.Teams[0].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamAUnconvertedTry:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + TryAttempt(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(ScoredTry(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync($"{FailedConversion(gameEvent.Game.Teams[0].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamAConvertedTry:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - " +
                    TryAttempt(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(ScoredTry(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync($"{Converted(gameEvent.Game.Teams[0].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamAFailedDropGoal:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttempDropGoal(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(FailedDropGoal(gameEvent.Game.Teams[0].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamAScoredDropGoal:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttempDropGoal(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(ScoredDropGoal(gameEvent.Game.Teams[0].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamAMissedPenalty:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttemptedPenalty(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(MissedPenalty(gameEvent.Game.Teams[0].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamAScoredPenalty:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - " + AttempDropGoal(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(ScoredDropGoal(gameEvent.Game.Teams[0].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            //---------------------

            case GameEventType.TeamBFailedTry:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - " + TryAttempt(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync($"{FailedTry(gameEvent.Game.Teams[1].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamBUnconvertedTry:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - " + TryAttempt(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(ScoredTry(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync($"{FailedConversion(gameEvent.Game.Teams[1].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamBConvertedTry:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - " + TryAttempt(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(ScoredTry(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync($"{Converted(gameEvent.Game.Teams[1].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamBFailedDropGoal:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttempDropGoal(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(FailedDropGoal(gameEvent.Game.Teams[1].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamBScoredDropGoal:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttemptedPenalty(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(MissedPenalty(gameEvent.Game.Teams[1].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamBMissedPenalty:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttempDropGoal(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(FailedDropGoal(gameEvent.Game.Teams[1].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamBScoredPenalty:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttempDropGoal(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await channel.SendMessageAsync(ScoredDropGoal(gameEvent.Game.Teams[1].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;


            default:
                await channel.SendMessageAsync($"{gameEvent.Minute}' - Something happened: " +
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

    private async Task WaitDelay() { await Task.Delay(TimeSpan.FromSeconds(5)); }

    private async Task AnnounceIntervalAsync(IMessageChannel channel,
        GameEvent gameEvent,
        string teamA,
        string teamB)
    {
        await channel.SendMessageAsync($"Half-time is here. Game restarts in 15 minutes. " +
            $"Current score: {teamA} {gameEvent.TeamAScore} " +
            $"x {gameEvent.TeamBScore} {teamB}");
    }

    private string CurrentScore(GameEvent gameEvent)
        => $"Current score: {gameEvent.Game.Teams[0].Team.Country} {gameEvent.TeamAScore} " +
        $"x {gameEvent.TeamBScore} {gameEvent.Game.Teams[1].Team.Country}";

    private string TryAttempt(string team)
        => $"{team} has advanced in the ruck or smt idk and is attempting a try.";

    private string ScoredTry(string team)
        => $"{team} has put the ball on the floor and scores! 5 points added.";

    private string FailedTry(string team)
        => $"{team}'s player tripped and fell before reaching the line. No try was scored.";

    private string Converted(string team)
        => $"{team} has shot it correctly and converts! 2 points added.";

    private string FailedConversion(string team)
        => $"{team} shot it wide and failed the conversion. No conversion was scored.";

    private string AttempDropGoal(string team)
        => $"{team} created space for a drop goal chance.";

    private string ScoredDropGoal(string team)
        => $"{team} made no mistake and scored the drop goal! 3 points added.";

    private string FailedDropGoal(string team)
        => $"The defense held firm and stopped {team}'s drop goal attempt. No drop goal was scored.";

    private string AttemptedPenalty(string team)
        => $"{team} got a penalty advantage after hands were not released in the tackle.";

    private string ScoredPenalty(string team)
        => $"{team} converted the penalty cleanly and scores! 3 points added.";

    private string MissedPenalty(string team)
        => $"{team} struck it too low and missed the target. No penalty was scored.";
}