using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Utilitaries;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Application.Simulators;

public class SeasonThreeGameSimulator : ISpecificGameSimulator
{
    private readonly Random _random = new();
    private readonly List<PendingInjury> _pendingInjuries = [];
    private readonly List<PendingYellowCard> _pendingYellowCards = [];
    private readonly List<GameCheer> _cheers = [];

    private SeasonThreeStats? _teamAStats;
    private SeasonThreeStats? _teamBStats;

    public Result AddCheer(Cheer cheer, Game game)
    {
        if (cheer.Yell.Length > 1000)
        {
            return Result.Failure("Yell is too long", Errors.Invalid);
        }

        if (_cheers.Count(temp => temp.Cheer.UserId == cheer.UserId) >= 3)
        {
            return Result.Failure("You already cheered 3 times", Errors.Invalid);
        }

        if (_cheers.Any(temp => temp.Cheer.UserId == cheer.UserId
            && !temp.Resolved))
        {
            return Result.Failure("You are already cheering; please wait 2 game minutes " +
                "before cheering again", Errors.Invalid);
        }

        cheer.StartMinute = game.CurrentMinute + 1;
        _cheers.Add(new(false, cheer));
        return Result.Success();
    }

    public async Task SaveGameAsync(GameEvent gameEvent, AppDbContext db)
    {
        await db.Games
            .Where(temp => temp.Id == gameEvent.Game.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(temp => temp.CurrentMinute, gameEvent.Game.CurrentMinute)
                .SetProperty(temp => temp.TeamAScore, gameEvent.Game.TeamAScore)
                .SetProperty(temp => temp.TeamBScore, gameEvent.Game.TeamBScore)
            );

        if (gameEvent.EventType == GameEventType.Nothing) return;

        if (gameEvent.EventType.IsTeamAScoredTry)
            gameEvent.Game.Teams[0].Team.ScoredTriesCount++;

        if (gameEvent.EventType.IsTeamBScoredTry)
            gameEvent.Game.Teams[1].Team.ScoredTriesCount++;

        await db.Teams
            .Where(temp => temp.Id == gameEvent.Game.Teams[0].Team.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(temp => temp.PointsScored,
                    temp => temp.PointsScored + gameEvent.EventType.GetTeamAScoreChange())
                .SetPropertyIf(gameEvent.EventType.IsTeamBScoring,
                    temp => temp.PointsTaken,
                    temp => temp.PointsTaken + gameEvent.EventType.GetTeamBScoreChange())
                .SetPropertyIf(gameEvent.EventType.IsTeamAScoredTry,
                    temp => temp.ScoredTriesCount,
                    temp => temp.ScoredTriesCount + 1)
                .SetPropertyIf(gameEvent.EventType.IsTeamBScoredTry,
                    temp => temp.SufferedTriesCount,
                    temp => temp.SufferedTriesCount + 1));


        await db.Teams
            .Where(temp => temp.Id == gameEvent.Game.Teams[1].Team.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(temp => temp.PointsScored,
                    temp => temp.PointsScored + gameEvent.EventType.GetTeamBScoreChange())
                .SetPropertyIf(gameEvent.EventType.IsTeamAScoring,
                    temp => temp.PointsTaken,
                    temp => temp.PointsTaken + gameEvent.EventType.GetTeamAScoreChange())
                .SetPropertyIf(gameEvent.EventType.IsTeamBScoredTry,
                    temp => temp.ScoredTriesCount,
                    temp => temp.ScoredTriesCount + 1)
                .SetPropertyIf(gameEvent.EventType.IsTeamAScoredTry,
                    temp => temp.SufferedTriesCount,
                    temp => temp.SufferedTriesCount + 1));
    }

    public GameEvent SimulateNextMinute(Game game)
    {
        if (_teamAStats is null || _teamBStats is null)
        {
            _teamAStats = new(game.Teams[0], game.Teams[1]);
            _teamBStats = new(game.Teams[1], game.Teams[0]);

            CreatePlayers(game.Teams[0]);
            CreatePlayers(game.Teams[1]);

            if (game.Teams[0].Cake is not null)
            {
                _teamAStats.AddCakeBonus();
            }

            if (game.Teams[1].Cake is not null)
            {
                _teamBStats.AddCakeBonus();
            }

            if (game.Teams[0].HasMoraleBoost)
            {
                _teamAStats.AddMoraleBoostBonus();
            }

            if (game.Teams[1].HasMoraleBoost)
            {
                _teamBStats.AddMoraleBoostBonus();
            }
        }

        var injuryCheck = CheckPendingInjuries(game);
        if (injuryCheck is not null) return injuryCheck;

        var yellowCardCheck = CheckPendingYellowCards(game);
        if (yellowCardCheck is not null) return yellowCardCheck;

        var cheerCheck = CheckCheers(game);
        if (cheerCheck is not null) return cheerCheck;

        RandomEventList<GameEvent> eventList = new(new Random());

        eventList
            .Add(_teamAStats.GetTryAttemptChance(_teamBStats),
                () => HandleTryAttempt(_teamAStats, game, true))
            .Add(_teamBStats.GetTryAttemptChance(_teamAStats),
                () => HandleTryAttempt(_teamBStats, game, false))

            .Add(_teamAStats.GetDropGoalAttemptChance(_teamBStats),
                () => HandleDropGoalAttempt(_teamAStats, game, true))
            .Add(_teamBStats.GetDropGoalAttemptChance(_teamAStats),
                () => HandleDropGoalAttempt(_teamBStats, game, false))

            .Add(_teamAStats.GetPenaltyKickAttemptChance(_teamBStats),
                () => HandlePenaltyKickAttempt(_teamAStats, game, true))
            .Add(_teamBStats.GetPenaltyKickAttemptChance(_teamAStats),
                () => HandlePenaltyKickAttempt(_teamBStats, game, false))

            .Add(SeasonThreeStats.GetAlienAbductionChance(),
                () => HandlePlayerAbduction(game.Teams[0], game, true))
            .Add(SeasonThreeStats.GetAlienAbductionChance(),
                () => HandlePlayerAbduction(game.Teams[1], game, false))

            .Add(_teamAStats.GetInjurySufferChance(_teamBStats),
                () => HandlePlayerInjuryRisk(game.Teams[0], game, true))
            .Add(_teamBStats.GetInjurySufferChance(_teamAStats),
                () => HandlePlayerInjuryRisk(game.Teams[1], game, false))

            .Add(_teamAStats.GetOffenceCommitChance(_teamBStats),
                () => HandleOffence(game.Teams[0], game, true))
            .Add(_teamBStats.GetOffenceCommitChance(_teamAStats),
                () => HandleOffence(game.Teams[1], game, false))

            .AddFallback(() => new GameEvent(game.CurrentMinute,
                GameEventType.Nothing,
                game.TeamAScore,
                game.TeamBScore,
                game));

        return eventList.Draw();
    }

    public static void CreatePlayers(TeamGame team)
    {
        int minimum = 4
            + (team.HasMoraleBoost ? 1 : 0)
            + (team.Cake is not null ? 1 : 0);

        int teamTotal = team.Team.Insight + team.Team.Physique + team.Team.Technique;

        if (teamTotal < minimum * 15)
        {
            throw new InvalidOperationException(
                $"The team has {teamTotal} total stats, but at least " +
                $"{minimum * 15} are required.");
        }

        var random = Random.Shared;

        int[] playerTotals = Enumerable.Repeat(minimum, 15).ToArray();

        int extra = teamTotal - (minimum * 15);

        for (int i = 0; i < extra; i++)
        {
            playerTotals[random.Next(15)]++;
        }

        Array.Sort(playerTotals);
        Array.Reverse(playerTotals);

        int[] insight = new int[15];
        int[] physique = new int[15];
        int[] technique = new int[15];

        int remainingInsight = team.Team.Insight;
        int remainingPhysique = team.Team.Physique;
        int remainingTechnique = team.Team.Technique;

        for (int i = 0; i < 15; i++)
        {
            int playerTotal = playerTotals[i];

            int futureCapacity = 0;

            for (int j = i + 1; j < 15; j++)
                futureCapacity += playerTotals[j];

            int minInsight = Math.Max(
                0,
                remainingInsight - futureCapacity);

            int maxInsight = Math.Min(
                playerTotal,
                remainingInsight);

            maxInsight = Math.Min(
                maxInsight,
                playerTotal + futureCapacity
                    - remainingPhysique);

            maxInsight = Math.Min(
                maxInsight,
                playerTotal + futureCapacity
                    - remainingTechnique);

            if (minInsight > maxInsight)
            {
                throw new InvalidOperationException(
                    "Could not generate a valid player distribution.");
            }

            int playerInsight = random.Next(
                minInsight,
                maxInsight + 1);

            int remainingForPlayer =
                playerTotal - playerInsight;

            remainingInsight -= playerInsight;

            int minPhysique = Math.Max(
                0,
                remainingPhysique - futureCapacity);

            minPhysique = Math.Max(
                minPhysique,
                remainingForPlayer - remainingTechnique);

            int maxPhysique = Math.Min(
                remainingForPlayer,
                remainingPhysique);

            maxPhysique = Math.Min(
                maxPhysique,
                remainingForPlayer + futureCapacity
                    - remainingTechnique);

            if (minPhysique > maxPhysique)
            {
                throw new InvalidOperationException(
                    "Could not generate a valid player distribution.");
            }

            int playerPhysique = random.Next(
                minPhysique,
                maxPhysique + 1);

            int playerTechnique =
                remainingForPlayer - playerPhysique;

            if (playerTechnique < 0 ||
                playerTechnique > remainingTechnique)
            {
                throw new InvalidOperationException(
                    "Could not generate a valid player distribution.");
            }

            insight[i] = playerInsight;
            physique[i] = playerPhysique;
            technique[i] = playerTechnique;

            remainingPhysique -= playerPhysique;
            remainingTechnique -= playerTechnique;
        }

        for (int i = 0; i < 15; i++)
        {
            team.Players.Add(new Player(
                Guid.NewGuid(),
                i + 1,
                insight[i],
                physique[i],
                technique[i],
                IsOnField: true));
        }

        for (int i = 15; i < 23; i++)
        {
            int playerInsight = random.Next(minimum + 1);

            int remaining = minimum - playerInsight;

            int playerPhysique = random.Next(remaining + 1);

            int playerTechnique =
                remaining - playerPhysique;

            team.Players.Add(new Player(
                Guid.NewGuid(),
                i + 1,
                playerInsight,
                playerPhysique,
                playerTechnique,
                IsOnField: false));
        }
    }

    private GameEvent HandleTryAttempt(SeasonThreeStats attempter,
        Game game,
        bool isTeamA)
    {
        double trySuccessChance = attempter.GetTryScoreChance();
        double roll = _random.NextDouble();

        if (roll > trySuccessChance)
        {
            return new GameEvent(game.CurrentMinute,
                isTeamA ? GameEventType.TeamAFailedTry : GameEventType.TeamBFailedTry,
                game.TeamAScore,
                game.TeamBScore,
                game);
        }

        double conversionChance = attempter.GetConversionSuccessChance();
        roll = _random.NextDouble();

        if (roll > conversionChance)
        {
            if (isTeamA) game.TeamAScore += 5;
            else game.TeamBScore += 5;

            return new GameEvent(game.CurrentMinute,
                isTeamA ? GameEventType.TeamAUnconvertedTry : GameEventType.TeamBUnconvertedTry,
                game.TeamAScore,
                game.TeamBScore,
                game);
        }

        if (isTeamA) game.TeamAScore += 7;
        else game.TeamBScore += 7;

        return new GameEvent(game.CurrentMinute,
            isTeamA ? GameEventType.TeamAConvertedTry : GameEventType.TeamBConvertedTry,
            game.TeamAScore,
            game.TeamBScore,
            game);
    }

    private GameEvent HandleDropGoalAttempt(SeasonThreeStats attempter,
        Game game,
        bool isTeamA)
    {
        double successChance = attempter.GetDropGoalSuccessChance();
        double roll = _random.NextDouble();

        if (roll > successChance)
        {
            return new(game.CurrentMinute,
                isTeamA ? GameEventType.TeamAFailedDropGoal : GameEventType.TeamBFailedDropGoal,
                game.TeamAScore,
                game.TeamBScore,
                game);
        }

        if (isTeamA) game.TeamAScore += 3;
        else game.TeamBScore += 3;

        return new(game.CurrentMinute,
            isTeamA ? GameEventType.TeamAScoredDropGoal : GameEventType.TeamBScoredDropGoal,
            game.TeamAScore,
            game.TeamBScore,
            game);
    }

    private GameEvent HandlePenaltyKickAttempt(SeasonThreeStats attempter,
        Game game,
        bool isTeamA)
    {
        double successChance = attempter.GetPenaltyKickSuccessChance();
        double roll = _random.NextDouble();

        if (roll > successChance)
        {
            return new(game.CurrentMinute,
                isTeamA ? GameEventType.TeamAMissedPenalty : GameEventType.TeamBMissedPenalty,
                game.TeamAScore,
                game.TeamBScore,
                game);
        }

        if (isTeamA) game.TeamAScore += 3;
        else game.TeamBScore += 3;

        return new(game.CurrentMinute,
            isTeamA ? GameEventType.TeamAScoredPenalty : GameEventType.TeamBScoredPenalty,
            game.TeamAScore,
            game.TeamBScore,
            game);
    }

    private GameEvent HandlePlayerAbduction(TeamGame victimTeam, Game game, bool isTeamA)
    {
        Player playerToAbduct = ChooseRandomPlayerOnField(victimTeam);
        Player? replacementPlayer = ReplacePlayer(victimTeam, playerToAbduct.Id);

        if (isTeamA)
        {
            _teamAStats!.RemovePlayerStats(playerToAbduct);
            if (replacementPlayer is not null)
            {
                _teamAStats.AddPlayerStats(replacementPlayer);
            }
        }
        else
        {
            _teamBStats!.RemovePlayerStats(playerToAbduct);
            if (replacementPlayer is not null)
            {
                _teamBStats.AddPlayerStats(replacementPlayer);
            }
        }

        return new(game.CurrentMinute,
            isTeamA ? GameEventType.TeamAPlayerAbducted : GameEventType.TeamBPlayerAbducted,
            game.TeamAScore,
            game.TeamBScore,
            game)
        {
            PlayerInvolved = playerToAbduct,
            ReplacementPlayer = replacementPlayer
        };
    }

    private GameEvent HandlePlayerInjuryRisk(TeamGame team, Game game, bool isTeamA)
    {
        var injuredPlayer = ChooseRandomPlayerOnField(team);
        team.Players.RemoveAll(player => player.Id == injuredPlayer.Id);
        team.Players.Add(injuredPlayer with { IsOnField = false, CanJoinField = false });

        int decisionMinute = _random.Next(0, 5);

        double isSeriousChance;
        if (isTeamA) isSeriousChance = _teamAStats!.GetInjuryBeingSeriousChance();
        else isSeriousChance = _teamBStats!.GetInjuryBeingSeriousChance();

        double roll = _random.NextDouble();
        bool isSerious = roll <= isSeriousChance;

        _pendingInjuries.Add(new(injuredPlayer.Id, isTeamA, decisionMinute, isSerious));

        if (isTeamA) _teamAStats!.RemovePlayerStats(injuredPlayer);
        else _teamBStats!.RemovePlayerStats(injuredPlayer);

        return new(game.CurrentMinute,
            isTeamA ? GameEventType.TeamAPlayerRisksInjury : GameEventType.TeamBPlayerRisksInjury,
            game.TeamAScore,
            game.TeamBScore,
            game)
        {
            PlayerInvolved = injuredPlayer
        };
    }

    private GameEvent HandleOffence(TeamGame team, Game game, bool isTeamA)
    {
        double roll;

        var playersWithYellowCard = team
            .Players
            .Where(player => player.HasYellowCard && player.IsOnField)
            .ToList();

        if (playersWithYellowCard.Count == 0)
        {
            var offender = ChooseRandomPlayerOnField(team);
            var newOffender = offender with { IsOnField = false, CanJoinField = false };

            team.Players.RemoveAll(player => player.Id == offender.Id);
            team.Players.Add(newOffender);

            if (isTeamA) _teamAStats!.RemovePlayerStats(offender);
            else _teamBStats!.RemovePlayerStats(offender);

            roll = _random.NextDouble();

            if (roll <= SeasonThreeStats.GetCardBeingRedChance())
            {
                return new(game.CurrentMinute,
                    isTeamA ? GameEventType.TeamAPlayerRedCard : GameEventType.TeamBPlayerRedCard,
                    game.TeamAScore,
                    game.TeamBScore,
                    game)
                {
                    PlayerInvolved = offender
                };
            }

            _pendingYellowCards.Add(new(offender.Id, isTeamA, game.CurrentMinute + 10));
            team.Players.RemoveAll(player => player.Id == newOffender.Id);
            team.Players.Add(newOffender with { HasYellowCard = true });

            return new(game.CurrentMinute,
                isTeamA ? GameEventType.TeamAPlayerYellowCard : GameEventType.TeamBPlayerYellowCard,
                game.TeamAScore,
                game.TeamBScore,
                game)
            {
                PlayerInvolved = newOffender
            };
        }

        roll = _random.NextDouble();
        bool goesToSomeoneWithYellowCard = roll <= 0.5;

        var playersWithoutYellowCard = team
            .Players
            .Where(temp => !temp.HasYellowCard && temp.IsOnField)
            .ToList();

        int index;
        Player player;

        if (goesToSomeoneWithYellowCard || playersWithoutYellowCard.Count == 0)
        {
            index = _random.Next(0, playersWithYellowCard.Count);
            player = playersWithYellowCard[index];
            team.Players.RemoveAll(temp => temp.Id == player.Id);
            team.Players.Add(player with { IsOnField = false, CanJoinField = false });

            return new(game.CurrentMinute,
                isTeamA ? GameEventType.TeamAPlayerRedCard : GameEventType.TeamBPlayerRedCard,
                game.TeamAScore,
                game.TeamBScore,
                game)
            {
                PlayerInvolved = player
            };
        }

        index = _random.Next(0, playersWithoutYellowCard.Count);
        player = playersWithoutYellowCard[index];
        team.Players.RemoveAll(temp => temp.Id == player.Id);
        team.Players.Add(player with { IsOnField = false, CanJoinField = false, HasYellowCard = true });

        return new(game.CurrentMinute,
            isTeamA ? GameEventType.TeamAPlayerYellowCard : GameEventType.TeamBPlayerYellowCard,
            game.TeamAScore,
            game.TeamBScore,
            game)
        {
            PlayerInvolved = player
        };
    }

    private GameEvent? CheckPendingInjuries(Game game)
    {
        var injury = _pendingInjuries
            .FirstOrDefault(injury => injury.DecisionMinute == game.CurrentMinute);

        if (injury is null) return null;

        _pendingInjuries.Remove(injury);

        var team = injury.IsTeamA ? game.Teams[0] : game.Teams[1];
        var teamStats = injury.IsTeamA ? _teamAStats! : _teamBStats!;

        var injuriedPlayer = team.Players.First(player => player.Id == injury.PlayerId);

        if (!injury.IsSerious)
        {
            teamStats.AddPlayerStats(injuriedPlayer);

            team.Players.RemoveAll(player => player.Id == injuriedPlayer.Id);
            team.Players.Add(injuriedPlayer with { IsOnField = true });

            return new(game.CurrentMinute,
                injury.IsTeamA ?
                    GameEventType.TeamAPlayerNonSeriousInjury :
                    GameEventType.TeamBPlayerNonSeriousInjury,
                game.TeamAScore,
                game.TeamBScore,
                game)
            {
                PlayerInvolved = injuriedPlayer
            };
        }

        var replacement = ReplacePlayer(team, injury.PlayerId);
        if (replacement is not null) teamStats.AddPlayerStats(replacement);

        return new(game.CurrentMinute,
            injury.IsTeamA ?
                GameEventType.TeamAPlayerSeriousInjury :
                GameEventType.TeamBPlayerSeriousInjury,
            game.TeamAScore,
            game.TeamBScore,
            game)
        {
            PlayerInvolved = injuriedPlayer,
            ReplacementPlayer = replacement
        };
    }

    private GameEvent? CheckPendingYellowCards(Game game)
    {
        var yellowCard = _pendingYellowCards
            .FirstOrDefault(card => card.ReturnMinute == game.CurrentMinute);

        if (yellowCard is null) return null;

        var team = yellowCard.IsTeamA ? game.Teams[0] : game.Teams[1];
        var teamStats = yellowCard.IsTeamA ? _teamAStats! : _teamBStats!;

        var offender = team.Players.First(player => player.Id == yellowCard.PlayerId);

        team.Players.RemoveAll(player => player.Id == offender.Id);
        team.Players.Add(offender with { IsOnField = true });
        teamStats.AddPlayerStats(offender);

        _pendingYellowCards.Remove(yellowCard);

        return new(game.CurrentMinute,
            yellowCard.IsTeamA ?
                GameEventType.TeamAPlayerReturningFromYellowCard
                : GameEventType.TeamBPlayerReturningFromYellowCard,
            game.TeamAScore,
            game.TeamBScore,
            game)
        {
            PlayerInvolved = offender
        };
    }

    private GameEvent? CheckCheers(Game game)
    {
        var cheersToFinish = _cheers
            .Where(temp => temp.Cheer.StartMinute + 2 == game.CurrentMinute && !temp.Resolved)
            .ToList();

        foreach (var cheer in cheersToFinish)
        {
            var stats = cheer.Cheer.ForTeamA ? _teamAStats! : _teamBStats!;
            stats.RemoveCheer();
            cheer.Resolved = true;
        }

        var cheerToApply = _cheers
            .FirstOrDefault(temp => temp.Cheer.StartMinute == game.CurrentMinute
                && !temp.Resolved && !temp.Applied);

        if (cheerToApply is null) return null;

        var teamStats = cheerToApply.Cheer.ForTeamA ? _teamAStats! : _teamBStats!;

        teamStats.AddCheer();

        cheerToApply.Applied = true;

        return new(game.CurrentMinute,
            cheerToApply.Cheer.ForTeamA ?
                GameEventType.TeamAGetsCheer
                : GameEventType.TeamBGetsCheer,
            game.TeamAScore,
            game.TeamBScore,
            game)
        {
            Cheer = cheerToApply.Cheer
        };
    }

    private Player ChooseRandomPlayerOnField(TeamGame team)
    {
        var playersOnField = team
            .Players
            .Where(player => player.IsOnField)
            .ToList();

        return playersOnField[_random.Next(0, playersOnField.Count)];
    }

    private Player? ReplacePlayer(TeamGame team, Guid playerId)
    {
        var playerToRemove = team
            .Players
            .First(player => player.Id == playerId);
        team.Players.RemoveAll(player => player.Id == playerToRemove.Id);
        team.Players.Add(playerToRemove with { IsOnField = false, CanJoinField = false });

        var replacementPlayers = team
            .Players
            .Where(player => !player.IsOnField && player.CanJoinField)
            .ToList();

        if (replacementPlayers.Count == 0)
        {
            return null;
        }

        int index = _random.Next(0, replacementPlayers.Count);

        var replacement = team
            .Players
            .First(player => player.Id == replacementPlayers[index].Id);

        team.Players.RemoveAll(player => player.Id == replacement.Id);

        team.Players.Add(replacement with { IsOnField = true });

        return replacement;
    }

    public sealed record SeasonThreeStats
    {
        public int Insight { get; private set; }
        public int Physique { get; private set; }
        public int Technique { get; private set; }

        public SeasonThreeStats(TeamGame team, TeamGame opponent)
        {
            Insight = team.Team.Insight;
            Physique = team.Team.Physique;
            Technique = team.Team.Technique;

            if (team.Tactic == Tactics.None) return;

            if (team.Tactic == Tactics.General)
            {
                Insight++;
                Physique++;
                Technique++;
                return;
            }

            if (team.Tactic == opponent.Tactic) return;

            if (opponent.Tactic is Tactics.General or Tactics.None
                || team.Tactic.IsStrongerThan(opponent.Tactic))
            {
                if (team.Tactic == Tactics.Physique)
                {
                    Physique += 6;
                    if (team.Coach == Coaches.Physique) Physique += 4;
                }
                else if (team.Tactic == Tactics.Insight)
                {
                    Insight += 6;
                    if (team.Coach == Coaches.Insight) Insight += 4;
                }
                else if (team.Tactic == Tactics.Technique)
                {
                    Technique += 6;
                    if (team.Coach == Coaches.Technique) Technique += 4;
                }
            }
        }

        public void AddPlayerStats(Player player)
        {
            Insight += player.Insight;
            Physique += player.Physique;
            Technique += player.Technique;
        }

        public void RemovePlayerStats(Player player)
        {
            Insight -= player.Insight;
            Physique -= player.Physique;
            Technique -= player.Technique;
        }

        public void AddCheer()
        {
            Insight += 1;
            Physique += 1;
            Technique += 1;
        }

        public void RemoveCheer()
        {
            Insight -= 1;
            Physique -= 1;
            Technique -= 1;
        }

        public void AddCakeBonus()
        {
            Insight += 3;
            Technique += 3;
            Physique += 3;
        }

        public void AddMoraleBoostBonus()
        {
            Insight += 3;
            Physique += 3;
            Technique += 3;
        }


        //the Get...Chance methods return the percentages in the range 0.0-1.0

        public double GetTryAttemptChance(SeasonThreeStats opponent)
        {
            double chance = (((20.0 * Physique) - (11.0 * opponent.Physique)
                + (6.0 * Technique) - (3.0 * opponent.Technique)) / 40.0) / 100.0;

            return NumberOrBoundary(chance, 0.0, 1.0);
        }

        public double GetTryScoreChance()
        {
            double chance = (((7.0 * Insight)
                + (2.0 * Physique)
                + (3.0 * Technique)
                - 4.0)
                / 12.0) / 100.0;

            return NumberOrBoundary(chance, 0.0, 1.0);
        }

        public double GetConversionSuccessChance()
        {
            double chance = ((Insight + (11.0 * Technique) - 4) / 12.0) / 100.0;
            return NumberOrBoundary(chance, 0.0, 1.0);
        }

        public double GetDropGoalAttemptChance(SeasonThreeStats opponent)
        {
            double chance = ((121.0
                + (5.0 * Insight)
                + (4.0 * Technique)
                - (6.0 * opponent.Technique))
                / 34.0) / 100.0;

            return NumberOrBoundary(chance, 0.0, 1.0);
        }

        public double GetDropGoalSuccessChance() => GetConversionSuccessChance();

        public double GetPenaltyKickAttemptChance(SeasonThreeStats opponent)
        {
            double chance = ((643.0
                + (5.0 * Insight)
                - (12.0 * opponent.Insight)
                + (3.0 * Physique)
                - (8.0 * opponent.Technique)) / 20.0) / 100.0;

            return NumberOrBoundary(chance, 0.0, 1.0);
        }

        public double GetPenaltyKickSuccessChance() => GetConversionSuccessChance();

        public double GetInjurySufferChance(SeasonThreeStats opponent)
        {
            double chance = ((1000.0
                 - (2.0 * Technique)
                 - (10.0 * Physique)
                 + (4.0 * opponent.Physique)
                 - (4.0 * Insight)) / 160.0) / 100.0;

            return NumberOrBoundary(chance, 0.0, 1.0);
        }

        public double GetInjuryBeingSeriousChance()
        {
            double chance = ((29800.0 - (300.0 * Insight)) / 396.0) / 100.0;

            return NumberOrBoundary(chance, 0.0, 1.0);
        }

        public double GetOffenceCommitChance(SeasonThreeStats opponent)
        {
            double chance = ((800.0
                - (7.0 * Insight)
                - (3.0 * Physique)
                + opponent.Insight) / 500.0) / 100.0;

            return NumberOrBoundary(chance, 0.0, 1.0);
        }

        public static double GetCardBeingRedChance() => 0.2;

        public static double GetAlienAbductionChance() => 0.05 / 100.0;

        private static double NumberOrBoundary(double number,
            double min,
            double max)
        {
            if (number < min) return min;
            if (number > max) return max;
            return number;
        }
    }

    private sealed record PendingInjury(Guid PlayerId,
        bool IsTeamA,
        int DecisionMinute,
        bool IsSerious);

    private sealed record PendingYellowCard(Guid PlayerId,
        bool IsTeamA,
        int ReturnMinute);

    private sealed record GameCheer
    {
        public bool Applied { get; set; } = false;
        public bool Resolved { get; set; }
        public Cheer Cheer { get; set; } = null!;

        public GameCheer(bool resolved, Cheer cheer)
        {
            Resolved = resolved;
            Cheer = cheer;
        }
    };
}