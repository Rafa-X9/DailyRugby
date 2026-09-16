using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Utilitaries;
using DailyRugby.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DailyRugby.Application.Simulators;

public class SeasonThreeGameSimulator : ISpecificGameSimulator
{
    private readonly Random _random = new();
    private readonly List<PendingInjury> _pendingInjuries = [];
    private SeasonThreeStats? _teamAStats;
    private SeasonThreeStats? _teamBStats;

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
        }

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
            + (team.IsUsingCake ? 1 : 0);

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
        _pendingInjuries.Add(new(injuredPlayer.Id, isTeamA, decisionMinute));

        return new(game.CurrentMinute,
            isTeamA ? GameEventType.TeamAPlayerRisksInjury : GameEventType.TeamBPlayerRisksInjury,
            game.TeamAScore,
            game.TeamBScore,
            game);
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
        public int Insight { get; init; }
        public int Physique { get; init; }
        public int Technique { get; init; }

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
        int DecisionMinute);
}