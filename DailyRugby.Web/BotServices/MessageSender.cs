using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using Discord;

namespace DailyRugby.Web.BotServices;

public class MessageSender
{
    private readonly IConfiguration _configuration;
    private readonly ulong _channelId;
    private bool _hasInitialized = false;
    private IMessageChannel _channel = null!;

    public MessageSender(IGameSimulatorManager simulator, IConfiguration configuration)
    {
        simulator.GameEventHappened += OnGameEventHappened;
        _configuration = configuration;
        _channelId = ulong.Parse(_configuration["ChannelId"] ?? throw new Exception());
    }

    private async void OnGameEventHappened(object? sender, EventArgs e)
    {
        var gameEvent = (GameEvent)e;
        if (!_hasInitialized)
        {
            _channel = (IMessageChannel)await Program.DiscordClient.GetChannelAsync(_channelId);
            _hasInitialized = true;
        }

        switch (gameEvent.EventType)
        {
            case GameEventType.GameStarted:
                await AnnounceGameStartAsync(_channel, gameEvent);
                break;

            case GameEventType.GameFinished:
                await AnnounceGameEndAsync(_channel, gameEvent);
                break;

            case GameEventType.HalfTime:
                await AnnounceIntervalAsync(_channel,
                    gameEvent,
                    gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country);
                break;

            //------------------------------

            case GameEventType.TeamAFailedTry:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + TryAttempt(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync($"{FailedTry(gameEvent.Game.Teams[0].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamAUnconvertedTry:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + TryAttempt(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(ScoredTry(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync($"{FailedConversion(gameEvent.Game.Teams[0].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamAConvertedTry:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - " +
                    TryAttempt(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(ScoredTry(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync($"{Converted(gameEvent.Game.Teams[0].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamAFailedDropGoal:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttempDropGoal(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(FailedDropGoal(gameEvent.Game.Teams[0].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamAScoredDropGoal:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttempDropGoal(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(ScoredDropGoal(gameEvent.Game.Teams[0].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamAMissedPenalty:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttemptedPenalty(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(MissedPenalty(gameEvent.Game.Teams[0].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamAScoredPenalty:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttemptedPenalty(gameEvent.Game.Teams[0].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(ScoredPenalty(gameEvent.Game.Teams[0].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            //---------------------

            case GameEventType.TeamBFailedTry:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + TryAttempt(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync($"{FailedTry(gameEvent.Game.Teams[1].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamBUnconvertedTry:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + TryAttempt(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(ScoredTry(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync($"{FailedConversion(gameEvent.Game.Teams[1].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamBConvertedTry:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + TryAttempt(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(ScoredTry(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync($"{Converted(gameEvent.Game.Teams[1].Team.Country)} " +
                    $"{CurrentScore(gameEvent)}");
                break;

            case GameEventType.TeamBFailedDropGoal:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttempDropGoal(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(FailedDropGoal(gameEvent.Game.Teams[1].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamBScoredDropGoal:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttempDropGoal(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(ScoredDropGoal(gameEvent.Game.Teams[1].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamBMissedPenalty:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttemptedPenalty(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(MissedPenalty(gameEvent.Game.Teams[1].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamBScoredPenalty:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - "
                    + AttemptedPenalty(gameEvent.Game.Teams[1].Team.Country));
                await WaitDelay();
                await _channel.SendMessageAsync(ScoredPenalty(gameEvent.Game.Teams[1].Team.Country)
                    + " " + CurrentScore(gameEvent));
                break;

            case GameEventType.TeamAPlayerAbducted:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - " +
                    $"A player from {gameEvent.Game.Teams[0].Team.Country} " +
                    $"is abducted by aliens! This is player " +
                    $"#{gameEvent.PlayerInvolved?.Number.ToString() ?? "UNKOWN"}. " +
                    $"He is replaced by #{gameEvent.ReplacementPlayer?.Number.ToString() ?? "UNKOWN"}");
                break;

            case GameEventType.TeamBPlayerAbducted:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - " +
                    $"A player from {gameEvent.Game.Teams[1].Team.Country} " +
                    $"is abducted by aliens! This is player " +
                    $"#{gameEvent.PlayerInvolved?.Number.ToString() ?? "UNKOWN"}. " +
                    $"He is replaced by #{gameEvent.ReplacementPlayer?.Number.ToString() ?? "UNKOWN"}");
                break;

            //-------

            case GameEventType.TeamAPlayerRisksInjury:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - A player from " +
                    $"{gameEvent.Game.Teams[0].Team.Country} was slapped on the face by " +
                    $"Will Smith and needed to leave the game while the doctors assess if " +
                    $"he can continue. This is #{gameEvent.PlayerInvolved?.Number.ToString()
                    ?? "UNKOWN"}.");
                break;

            case GameEventType.TeamBPlayerRisksInjury:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - A player from " +
                    $"{gameEvent.Game.Teams[1].Team.Country} was slapped on the face by " +
                    $"Will Smith and needed to leave the game while the doctors assess if " +
                    $"he can continue. This is #{gameEvent.PlayerInvolved?.Number.ToString()
                    ?? "UNKOWN"}.");
                break;

            case GameEventType.TeamAPlayerNonSeriousInjury:
                await _channel.SendMessageAsync($"The injured player " +
                    $"from {gameEvent.Game.Teams[0].Team.Country} was deemed to have deserved " +
                    $"the slap, so he was sent back to the field. This is #{gameEvent
                    .PlayerInvolved?.Number.ToString() ?? "UNKOWN"}. He is back on the game.");
                break;


            case GameEventType.TeamBPlayerNonSeriousInjury:
                await _channel.SendMessageAsync($"The injured player " +
                    $"from {gameEvent.Game.Teams[1].Team.Country} was deemed to have deserved " +
                    $"the slap, so he was sent back to the field. This is #{gameEvent
                    .PlayerInvolved?.Number.ToString() ?? "UNKOWN"}. He is back on the game.");
                break;

            case GameEventType.TeamAPlayerSeriousInjury:
                await _channel.SendMessageAsync($"The injured player " +
                    $"from {gameEvent.Game.Teams[0].Team.Country} was slapped so hard he died. " +
                    $"This is #{gameEvent.PlayerInvolved?.Number.ToString() ?? "UNKOWN"}. He is " +
                    $"replaced by #{gameEvent.ReplacementPlayer?.Number.ToString() ?? "UNKNOWN"}.");
                break;

            case GameEventType.TeamBPlayerSeriousInjury:
                await _channel.SendMessageAsync($"The injured player " +
                    $"from {gameEvent.Game.Teams[1].Team.Country} was slapped so hard he died. " +
                    $"This is #{gameEvent.PlayerInvolved?.Number.ToString() ?? "UNKOWN"}. He is " +
                    $"replaced by #{gameEvent.ReplacementPlayer?.Number.ToString() ?? "UNKNOWN"}.");
                break;

            //-------------

            case GameEventType.TeamAPlayerYellowCard:
                await _channel.SendMessageAsync($"{gameEvent.Game.CurrentMinute}' - A player from " +
                    $"{gameEvent.Game.Teams[0].Team.Country} " +
                    $"punched an opponent in the face. He receives a yellow card, and is therefore out " +
                    $"of the field for 10 minutes. This is {gameEvent.PlayerInvolved?.Number.ToString()
                    ?? "UNKONWN"}.");
                break;

            case GameEventType.TeamAPlayerReturningFromYellowCard:
                await _channel.SendMessageAsync($"#{gameEvent.PlayerInvolved?.Number.ToString() ??
                    "UNKONW"} from {gameEvent.Game.Teams[0].Team.Country} is back from his yellow card.");
                break;

            case GameEventType.TeamAPlayerRedCard:
                await _channel.SendMessageAsync($"{gameEvent.Game.CurrentMinute}' - A player from " +
                    $"{gameEvent.Game.Teams[0].Team.Country} was seen downvoting comments on r/dailygames. " +
                    $"He is immediatelly given a red card. This is #{gameEvent.PlayerInvolved?.Number
                    .ToString() ?? "UNKOWN."}");
                break;


            case GameEventType.TeamBPlayerYellowCard:
                await _channel.SendMessageAsync($"{gameEvent.Game.CurrentMinute}' - A player from " +
                    $"{gameEvent.Game.Teams[1].Team.Country} " +
                    $"punched an opponent in the face. He receives a yellow card, and is therefore out " +
                    $"of the field for 10 minutes. This is {gameEvent.PlayerInvolved?.Number.ToString()
                    ?? "UNKONWN"}.");
                break;

            case GameEventType.TeamBPlayerReturningFromYellowCard:
                await _channel.SendMessageAsync($"#{gameEvent.PlayerInvolved?.Number.ToString() ??
                    "UNKONW"} from {gameEvent.Game.Teams[1].Team.Country} is back from his yellow card.");
                break;

            case GameEventType.TeamBPlayerRedCard:
                await _channel.SendMessageAsync($"{gameEvent.Game.CurrentMinute}' - A player from " +
                    $"{gameEvent.Game.Teams[1].Team.Country} was seen downvoting comments on r/dailygames. " +
                    $"He is immediatelly given a red card. This is #{gameEvent.PlayerInvolved?.Number
                    .ToString() ?? "UNKOWN."}");
                break;

            // -----------

            case GameEventType.TeamAGetsCheer:
                string yell = "!";

                if (gameEvent.Cheer?.Yell is { Length: > 0 })
                {
                    yell = ": " + gameEvent.Cheer.Yell;
                }
                await _channel.SendMessageAsync($"<@{gameEvent.Cheer?.UserId.ToString() ?? "???"}> " +
                    $"is cheering for {gameEvent.Game.Teams[0].Team.Country}{yell}");
                break;

            case GameEventType.TeamBGetsCheer:
                yell = "!";

                if (gameEvent.Cheer?.Yell is { Length: > 0 })
                {
                    yell = ": " + gameEvent.Cheer.Yell;
                }
                await _channel.SendMessageAsync($"<@{gameEvent.Cheer?.UserId.ToString() ?? "???"}> " +
                    $"is cheering for {gameEvent.Game.Teams[1].Team.Country}{yell}");
                break;


            default:
                await _channel.SendMessageAsync($"{gameEvent.Minute}' - Something happened: " +
                    $"{gameEvent.EventType}");
                break;
        }
    }

    private async Task AnnounceGameStartAsync(IMessageChannel channel, GameEvent gameEvent)
    {
        await channel.SendMessageAsync($"The game between {gameEvent.Game.Teams[0].Team.Country} and " +
            $"{gameEvent.Game.Teams[1].Team.Country} begins!");

        await channel.SendMessageAsync($"{gameEvent.Game.Teams[0].Team.Country} has chosen the " +
            $"{gameEvent.Game.Teams[0].Tactic} tactic, while {gameEvent.Game.Teams[1].Team.Country} " +
            $"has chosen the {gameEvent.Game.Teams[1].Tactic} tactic");
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