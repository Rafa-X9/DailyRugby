using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using Discord;
using System.Globalization;
using System.Text;

namespace DailyRugby.Web.BotServices;

public class MessageSender
{
    private readonly IConfiguration _configuration;
    private readonly ulong _channelId;
    private bool _hasInitialized = false;
    private IMessageChannel _channel = null!;
    private readonly MessageProvider _messageProvider;
    private readonly List<PendingInjuries> _pendingInjuries = [];
    private readonly IGameOddsCalculator _gameOddsCalculator;

    private sealed record PendingInjuries(Guid PlayerId,
        InjuryRiskMessage Message);

    public MessageSender(IGameSimulatorManager simulator,
        IConfiguration configuration,
        IGameOddsCalculator gameOddsCalculator,
        MessageProvider messageProvider)
    {
        simulator.GameEventHappened += OnGameEventHappened;
        _configuration = configuration;
        _channelId = ulong.Parse(_configuration["DailyRugby:ChannelId"] ?? throw new Exception());
        _messageProvider = messageProvider;
        _gameOddsCalculator = gameOddsCalculator;
    }

    private async void OnGameEventHappened(object? sender, EventArgs e)
    {
        var gameEvent = (GameEvent)e;
        if (!_hasInitialized)
        {
            await SetupChannelAsync();
        }

        switch (gameEvent.EventType)
        {
            case GameEventType.GameStarted:
                await AnnounceGameStartAsync(_channel, gameEvent);

                var oddsTask = _gameOddsCalculator.GetOddsAsync(gameEvent.Game.Id, true);
                await WaitDelay();
                var oddsResult = await oddsTask;
                double teamAWins, teamBWins;

                if (oddsResult.IsSuccessful)
                {
                    teamAWins = ((double)oddsResult.Item.TeamAWins
                        / oddsResult.Item.TotalSimulations) * 100;

                    teamBWins = ((double)oddsResult.Item.TeamBWins
                        / oddsResult.Item.TotalSimulations) * 100;

                    CultureInfo ci = CultureInfo.InvariantCulture;

                    await _channel.SendMessageAsync($"{gameEvent.Game.Teams[0].Team.Country} has " +
                        $"{teamAWins.ToString("F0", ci)}% chance to win, while {gameEvent.Game.Teams[1]
                        .Team.Country} has {teamBWins.ToString("F0", ci)}%");
                }

                string teamATacticMessage;

                if (gameEvent.Game.Teams[0].Tactic == Tactics.None)
                {
                    teamATacticMessage = "no tactic";
                }
                else
                {
                    teamATacticMessage = $"the {gameEvent.Game.Teams[0].Tactic
                        .ToString().ToLower()} tactic";
                }

                string teamBTacticMessage;

                if (gameEvent.Game.Teams[1].Tactic == Tactics.None)
                {
                    teamBTacticMessage = "no tactic";
                }
                else
                {
                    teamBTacticMessage = $"the {gameEvent.Game.Teams[1].Tactic
                        .ToString().ToLower()} tactic";
                }

                await _channel.SendMessageAsync($"{gameEvent.Game.Teams[0].Team.Country} is using " +
                $"{teamATacticMessage}, while {gameEvent.Game.Teams[1].Team.Country} is using " +
                $"{teamBTacticMessage}.");

                StringBuilder cakesMessage = new();

                if (gameEvent.Game.Teams[0].Cake is not null)
                {
                    cakesMessage.Append($"{gameEvent.Game.Teams[0].Team.Country}'s coach " +
                        $"bought a {gameEvent.Game.Teams[0].Cake!.Name} cake for the team, " +
                        $"giving them a morale boost.");
                }
                if (gameEvent.Game.Teams[1].Cake is not null)
                {
                    cakesMessage.Append($"{gameEvent.Game.Teams[1].Team.Country}'s coach " +
                        $"bought a {gameEvent.Game.Teams[1].Cake!.Name} cake for the team, " +
                        $"giving them a morale boost.");
                }

                if (!string.IsNullOrWhiteSpace(cakesMessage.ToString()))
                {
                    await WaitDelay();
                    await _channel.SendMessageAsync(cakesMessage.ToString());
                }

                var specificOddsTask = _gameOddsCalculator.GetSpecificOddsAsync(
                    gameEvent.Game.Id,
                    gameEvent.Game.Teams[0].Tactic,
                    gameEvent.Game.Teams[1].Tactic,
                    gameEvent.Game.Teams[0].Cake is not null,
                    gameEvent.Game.Teams[1].Cake is not null);

                await WaitDelay();
                var specificOdds = await specificOddsTask;

                if (specificOdds.IsSuccessful)
                {
                    teamAWins = ((double)specificOdds.Item.TeamAWins
                        / specificOdds.Item.TotalSimulations) * 100;

                    teamBWins = ((double)specificOdds.Item.TeamBWins
                        / specificOdds.Item.TotalSimulations) * 100;

                    var ci = CultureInfo.InvariantCulture;

                    await _channel.SendMessageAsync("According to the bookmarkers, this changes the odds " +
                        $"of the game so now {gameEvent.Game.Teams[0].Team.Country} has {teamAWins
                        .ToString("F0", ci)}% chance to win and {gameEvent.Game.Teams[1].Team.Country} has " +
                        $"{teamBWins.ToString("F0", ci)}%.");
                }

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
                var injuryRiskMessage = _messageProvider
                    .GetInjuryRiskMessage(gameEvent.Game.Teams[0].Team.Country,
                        gameEvent.Game.Teams[1].Team.Country);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {injuryRiskMessage
                    .Description} This is #{gameEvent.PlayerInvolved?.Number.ToString() ??
                    "UNKNOWN"}.");

                if (gameEvent.PlayerInvolved is not null)
                {
                    _pendingInjuries.Add(new(gameEvent.PlayerInvolved.Id, injuryRiskMessage));
                }

                break;

            case GameEventType.TeamBPlayerRisksInjury:
                injuryRiskMessage = _messageProvider
                    .GetInjuryRiskMessage(gameEvent.Game.Teams[1].Team.Country,
                        gameEvent.Game.Teams[0].Team.Country);

                await _channel.SendMessageAsync($"{gameEvent.Minute}' - {injuryRiskMessage
                    .Description} This is #{gameEvent.PlayerInvolved?.Number.ToString() ??
                    "UNKNOWN"}.");

                if (gameEvent.PlayerInvolved is not null)
                {
                    _pendingInjuries.Add(new(gameEvent.PlayerInvolved.Id, injuryRiskMessage));
                }

                break;

            case GameEventType.TeamAPlayerNonSeriousInjury:

                var injury = _pendingInjuries
                    .FirstOrDefault(temp => gameEvent.PlayerInvolved is not null
                        && temp.PlayerId == gameEvent.PlayerInvolved.Id);

                if (injury is null)
                {
                    await _channel.SendMessageAsync($"A player from {gameEvent.Game.Teams[0]
                        .Team.Country} who had left the field to assess an injury was found " +
                        "to not have a serious injury. He returns to the field.");
                    return;
                }

                var player = gameEvent.Game.Teams[0].Players.First(temp => temp.Id == injury.PlayerId);

                _pendingInjuries.Remove(injury);
                await _channel.SendMessageAsync($"{injury.Message.DescriptionRecovered} This is " +
                    $"#{player.Number}. He is back on field.");

                break;

            case GameEventType.TeamBPlayerNonSeriousInjury:

                injury = _pendingInjuries
                    .FirstOrDefault(temp => gameEvent.PlayerInvolved is not null
                        && temp.PlayerId == gameEvent.PlayerInvolved.Id);

                if (injury is null)
                {
                    await _channel.SendMessageAsync($"A player from {gameEvent.Game.Teams[1]
                        .Team.Country} who had left the field to assess an injury was found " +
                        "to not have a serious injury. He returns to the field.");
                    return;
                }

                player = gameEvent.Game.Teams[1].Players.First(temp => temp.Id == injury.PlayerId);

                _pendingInjuries.Remove(injury);
                await _channel.SendMessageAsync($"{injury.Message.DescriptionRecovered} This is " +
                    $"#{player.Number}. He is back on field.");

                break;

            case GameEventType.TeamAPlayerSeriousInjury:

                injury = _pendingInjuries
                    .FirstOrDefault(temp => gameEvent.PlayerInvolved is not null
                        && temp.PlayerId == gameEvent.PlayerInvolved.Id);

                if (injury is null)
                {
                    await _channel.SendMessageAsync($"A player from {gameEvent.Game.Teams[0]
                        .Team.Country} who had left the field to assess an injury was found " +
                        $"to have a serious injury. He is replaced by #{gameEvent.ReplacementPlayer
                        ?.Number.ToString() ?? "UNKNOWN"}");
                    return;
                }

                player = gameEvent.Game.Teams[0].Players.First(temp => temp.Id == injury.PlayerId);

                _pendingInjuries.Remove(injury);
                await _channel.SendMessageAsync($"{injury.Message.DescriptionRecovered} This is " +
                    $"#{player.Number}. He is replaced by #{gameEvent.ReplacementPlayer?.Number
                    .ToString() ?? "UNKNOWN"}.");

                break;

            case GameEventType.TeamBPlayerSeriousInjury:

                injury = _pendingInjuries
                    .FirstOrDefault(temp => gameEvent.PlayerInvolved is not null
                        && temp.PlayerId == gameEvent.PlayerInvolved.Id);

                if (injury is null)
                {
                    await _channel.SendMessageAsync($"A player from {gameEvent.Game.Teams[1]
                        .Team.Country} who had left the field to assess an injury was found " +
                        $"to have a serious injury. He is replaced by #{gameEvent.ReplacementPlayer
                        ?.Number.ToString() ?? "UNKNOWN"}");
                    return;
                }

                player = gameEvent.Game.Teams[1].Players.First(temp => temp.Id == injury.PlayerId);

                _pendingInjuries.Remove(injury);
                await _channel.SendMessageAsync($"{injury.Message.DescriptionRecovered} This is " +
                    $"#{player.Number}. He is replaced by #{gameEvent.ReplacementPlayer?.Number
                    .ToString() ?? "UNKNOWN"}.");

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

        List<int> chosenNumbers = [.. set];

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

    private async Task SetupChannelAsync()
    {
        bool isThreadParsed = bool.TryParse(_configuration["DailyRugby:IsThread"],
                out bool isThread);

        if (!isThreadParsed)
        {
            throw new Exception("IsThread configuration value is required.");
        }

        var channel = await Program.DiscordClient.GetChannelAsync(_channelId);

        if (isThread)
            _channel = (IThreadChannel)channel;
        else
            _channel = (IMessageChannel)channel;

        _hasInitialized = true;
    }

    public async Task SendMessageAsync(string message)
    {
        if (!_hasInitialized) await SetupChannelAsync();
        await _channel.SendMessageAsync(message);
    }
}