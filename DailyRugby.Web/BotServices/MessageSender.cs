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
    private readonly MessageProvider _messageProvider;

    public MessageSender(IGameSimulatorManager simulator,
        IConfiguration configuration,
        MessageProvider messageProvider)
    {
        simulator.GameEventHappened += OnGameEventHappened;
        _configuration = configuration;
        _channelId = ulong.Parse(_configuration["ChannelId"] ?? throw new Exception());
        _messageProvider = messageProvider;
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
                var (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[0]);

                TryAttemptMessage tryAttemptMessage = _messageProvider
                    .GetTryAttemptMessage(gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country,
                    n1, n2, n3);

                int seconds = Random.Shared.Next(tryAttemptMessage.MinSeconds,
                    tryAttemptMessage.MaxSeconds + 1);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {tryAttemptMessage.Description}");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                await _channel.SendMessageAsync($"{tryAttemptMessage.Failure} {CurrentScore(gameEvent)}.");

                break;

            case GameEventType.TeamAUnconvertedTry:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[0]);

                tryAttemptMessage = _messageProvider
                    .GetTryAttemptMessage(gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country,
                    n1, n2, n3);

                seconds = Random.Shared.Next(tryAttemptMessage.MinSeconds,
                    tryAttemptMessage.MaxSeconds + 1);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {tryAttemptMessage.Description}");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                await _channel.SendMessageAsync($"{tryAttemptMessage.Success}");
                await Task.Delay(TimeSpan.FromSeconds(5));
                await _channel.SendMessageAsync($"{_messageProvider.GetConversionFailureMessage(
                    gameEvent.Game.Teams[0].Team.Country)} {CurrentScore(gameEvent)}.");

                break;

            case GameEventType.TeamAConvertedTry:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[0]);

                tryAttemptMessage = _messageProvider
                    .GetTryAttemptMessage(gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country,
                    n1, n2, n3);

                seconds = Random.Shared.Next(tryAttemptMessage.MinSeconds,
                    tryAttemptMessage.MaxSeconds + 1);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {tryAttemptMessage.Description}");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                await _channel.SendMessageAsync($"{tryAttemptMessage.Success}");
                await Task.Delay(TimeSpan.FromSeconds(5));
                await _channel.SendMessageAsync($"{_messageProvider.GetConversionSuccessMessage(
                    gameEvent.Game.Teams[0].Team.Country)} {CurrentScore(gameEvent)}.");

                break;

            case GameEventType.TeamAFailedDropGoal:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[0]);
                
                var dropGoalMessage = _messageProvider.GetDropGoalMessage(
                    gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country,
                    n1, n2, n3);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - " +
                    $"{dropGoalMessage.Description}");

                seconds = Random.Shared.Next(dropGoalMessage.MinSeconds,
                    dropGoalMessage.MaxSeconds + 1);
                await Task.Delay(TimeSpan.FromSeconds(seconds));

                await _channel.SendMessageAsync($"{dropGoalMessage.Failure} " +
                    $"{CurrentScore(gameEvent)}");

                break;

            case GameEventType.TeamAScoredDropGoal:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[0]);

                dropGoalMessage = _messageProvider.GetDropGoalMessage(
                    gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country,
                    n1, n2, n3);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - " +
                    $"{dropGoalMessage.Description}");

                seconds = Random.Shared.Next(dropGoalMessage.MinSeconds,
                    dropGoalMessage.MaxSeconds + 1);
                await Task.Delay(TimeSpan.FromSeconds(seconds));

                await _channel.SendMessageAsync($"{dropGoalMessage.Success} " +
                    $"{CurrentScore(gameEvent)}");

                break;

            case GameEventType.TeamAMissedPenalty:

                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[0]);

                var penaltyMessage = _messageProvider.GetPenaltyMessage(
                    gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country,
                    n1, n2, n3);

                await _channel.SendMessageAsync($"{gameEvent.Game.CurrentMinute}' - " +
                    $"{penaltyMessage.Description}");

                seconds = Random.Shared.Next(penaltyMessage.MinSeconds,
                    penaltyMessage.MaxSeconds + 1);
                await Task.Delay(TimeSpan.FromSeconds(seconds));

                await _channel.SendMessageAsync($"{penaltyMessage.Failure} " +
                    $"{CurrentScore(gameEvent)}");

                break;

            case GameEventType.TeamAScoredPenalty:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[0]);

                penaltyMessage = _messageProvider.GetPenaltyMessage(
                    gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country,
                    n1, n2, n3);

                await _channel.SendMessageAsync($"{gameEvent.Game.CurrentMinute}' - " +
                    $"{penaltyMessage.Description}");

                seconds = Random.Shared.Next(penaltyMessage.MinSeconds,
                    penaltyMessage.MaxSeconds + 1);
                await Task.Delay(TimeSpan.FromSeconds(seconds));

                await _channel.SendMessageAsync($"{penaltyMessage.Success} " +
                    $"{CurrentScore(gameEvent)}");

                break;

            //---------------------

            case GameEventType.TeamBFailedTry:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[1]);

                tryAttemptMessage = _messageProvider
                    .GetTryAttemptMessage(gameEvent.Game.Teams[1].Team.Country,
                    gameEvent.Game.Teams[0].Team.Country,
                    n1, n2, n3);

                seconds = Random.Shared.Next(tryAttemptMessage.MinSeconds,
                    tryAttemptMessage.MaxSeconds + 1);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {tryAttemptMessage.Description}");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                await _channel.SendMessageAsync($"{tryAttemptMessage.Failure} {CurrentScore(gameEvent)}.");

                break;

            case GameEventType.TeamBUnconvertedTry:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[1]);

                tryAttemptMessage = _messageProvider
                    .GetTryAttemptMessage(gameEvent.Game.Teams[1].Team.Country,
                    gameEvent.Game.Teams[0].Team.Country,
                    n1, n2, n3);

                seconds = Random.Shared.Next(tryAttemptMessage.MinSeconds,
                    tryAttemptMessage.MaxSeconds + 1);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {tryAttemptMessage.Description}");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                await _channel.SendMessageAsync($"{tryAttemptMessage.Success}");
                await Task.Delay(TimeSpan.FromSeconds(5));
                await _channel.SendMessageAsync($"{_messageProvider.GetConversionFailureMessage(
                    gameEvent.Game.Teams[1].Team.Country)} {CurrentScore(gameEvent)}.");

                break;

            case GameEventType.TeamBConvertedTry:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[1]);

                tryAttemptMessage = _messageProvider
                    .GetTryAttemptMessage(gameEvent.Game.Teams[1].Team.Country,
                    gameEvent.Game.Teams[0].Team.Country,
                    n1, n2, n3);

                seconds = Random.Shared.Next(tryAttemptMessage.MinSeconds,
                    tryAttemptMessage.MaxSeconds + 1);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {tryAttemptMessage.Description}");
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                await _channel.SendMessageAsync($"{tryAttemptMessage.Success}");
                await Task.Delay(TimeSpan.FromSeconds(5));
                await _channel.SendMessageAsync($"{_messageProvider.GetConversionSuccessMessage(
                    gameEvent.Game.Teams[1].Team.Country)} {CurrentScore(gameEvent)}.");

                break;

            case GameEventType.TeamBFailedDropGoal:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[1]);

                dropGoalMessage = _messageProvider.GetDropGoalMessage(
                    gameEvent.Game.Teams[1].Team.Country,
                    gameEvent.Game.Teams[0].Team.Country,
                    n1, n2, n3);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - " +
                    $"{dropGoalMessage.Description}");

                seconds = Random.Shared.Next(dropGoalMessage.MinSeconds,
                    dropGoalMessage.MaxSeconds + 1);
                await Task.Delay(TimeSpan.FromSeconds(seconds));

                await _channel.SendMessageAsync($"{dropGoalMessage.Failure} " +
                    $"{CurrentScore(gameEvent)}");

                break;

            case GameEventType.TeamBScoredDropGoal:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[1]);

                dropGoalMessage = _messageProvider.GetDropGoalMessage(
                    gameEvent.Game.Teams[1].Team.Country,
                    gameEvent.Game.Teams[0].Team.Country,
                    n1, n2, n3);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - " +
                    $"{dropGoalMessage.Description}");

                seconds = Random.Shared.Next(dropGoalMessage.MinSeconds,
                    dropGoalMessage.MaxSeconds + 1);
                await Task.Delay(TimeSpan.FromSeconds(seconds));

                await _channel.SendMessageAsync($"{dropGoalMessage.Success} " +
                    $"{CurrentScore(gameEvent)}");

                break;

            case GameEventType.TeamBMissedPenalty:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[1]);

                penaltyMessage = _messageProvider.GetPenaltyMessage(
                    gameEvent.Game.Teams[1].Team.Country,
                    gameEvent.Game.Teams[0].Team.Country,
                    n1, n2, n3);

                await _channel.SendMessageAsync($"{gameEvent.Game.CurrentMinute}' - " +
                    $"{penaltyMessage.Description}");

                seconds = Random.Shared.Next(penaltyMessage.MinSeconds,
                    penaltyMessage.MaxSeconds + 1);
                await Task.Delay(TimeSpan.FromSeconds(seconds));

                await _channel.SendMessageAsync($"{penaltyMessage.Failure} " +
                    $"{CurrentScore(gameEvent)}");

                break;

            case GameEventType.TeamBScoredPenalty:
                (n1, n2, n3) = GetThreePlayersOnField(gameEvent.Game.Teams[1]);

                penaltyMessage = _messageProvider.GetPenaltyMessage(
                    gameEvent.Game.Teams[1].Team.Country,
                    gameEvent.Game.Teams[0].Team.Country,
                    n1, n2, n3);

                await _channel.SendMessageAsync($"{gameEvent.Game.CurrentMinute}' - " +
                    $"{penaltyMessage.Description}");

                seconds = Random.Shared.Next(penaltyMessage.MinSeconds,
                    penaltyMessage.MaxSeconds + 1);
                await Task.Delay(TimeSpan.FromSeconds(seconds));

                await _channel.SendMessageAsync($"{penaltyMessage.Success} " +
                    $"{CurrentScore(gameEvent)}");

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
                string yellowCardMessage = _messageProvider.GetYellowCardMessage(
                    gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {yellowCardMessage} " +
                    $"This is #{gameEvent.PlayerInvolved?.Number.ToString() ?? "UNKNOWN"}. He " +
                    $"is out of the field for 10 minutes.");

                break;

            case GameEventType.TeamAPlayerReturningFromYellowCard:
                await _channel.SendMessageAsync($"#{gameEvent.PlayerInvolved?.Number.ToString() ??
                    "UNKONW"} from {gameEvent.Game.Teams[0].Team.Country} is back from his yellow card.");
                break;

            case GameEventType.TeamAPlayerRedCard:
                string redCardMessage = _messageProvider.GetRedCardMessage(
                    gameEvent.Game.Teams[0].Team.Country,
                    gameEvent.Game.Teams[1].Team.Country);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {redCardMessage} " +
                    $"This is #{gameEvent.PlayerInvolved?.Number.ToString() ?? "UNKNOWN"}. He " +
                    $"is out of the field for the remaining of the game.");

                break;

            case GameEventType.TeamBPlayerYellowCard:
                yellowCardMessage = _messageProvider.GetYellowCardMessage(
                    gameEvent.Game.Teams[1].Team.Country,
                    gameEvent.Game.Teams[0].Team.Country);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {yellowCardMessage} " +
                    $"This is #{gameEvent.PlayerInvolved?.Number.ToString() ?? "UNKNOWN"}. He " +
                    $"is out of the field for 10 minutes.");

                break;

            case GameEventType.TeamBPlayerReturningFromYellowCard:
                await _channel.SendMessageAsync($"#{gameEvent.PlayerInvolved?.Number.ToString() ??
                    "UNKONW"} from {gameEvent.Game.Teams[1].Team.Country} is back from his yellow card.");
                break;

            case GameEventType.TeamBPlayerRedCard:
                redCardMessage = _messageProvider.GetRedCardMessage(
                    gameEvent.Game.Teams[1].Team.Country,
                    gameEvent.Game.Teams[0].Team.Country);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {redCardMessage} " +
                    $"This is #{gameEvent.PlayerInvolved?.Number.ToString() ?? "UNKNOWN"}. He " +
                    $"is out of the field for the remaining of the game.");

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

            case GameEventType.Nothing:
                if (Random.Shared.Next() % 2 == 0) break;

                int teamIndex = Random.Shared.Next() % 2 == 0 ? 0 : 1;

                string randomMessage = _messageProvider.GetRandomMessage(
                    gameEvent.Game.Teams[teamIndex].Team.Country);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - " +
                    $"{randomMessage}");

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

    private (int n1, int n2, int n3) GetThreePlayersOnField(TeamGame team)
    {
        int[] numbers = team.Players
            .Where(temp => temp.IsOnField)
            .Select(temp => temp.Number)
            .ToArray();

        HashSet<int> set = [];

        while (set.Count < 3)
        {
            set.Add(numbers[Random.Shared.Next(0, numbers.Length)]);
        }

        List<int> chosenNumbers = [..set];

        return (chosenNumbers[0], chosenNumbers[1], chosenNumbers[2]);
    }

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
}