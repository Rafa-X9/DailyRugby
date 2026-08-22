using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Utilitaries;
using DailyRugby.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DailyRugby.Application.Simulators;

public class SeasonOneGameSimulator : ISpecificGameSimulator
{
    private Stats? _teamAStats;
    private Stats? _teamBStats;
    private Stack<(GameEventType EventType, Action<Game> GameAction)>? _stack;

    public async Task SaveGameAsync(GameEvent gameEvent, AppDbContext db)
    {
        await db.Games
            .Where(temp => temp.Id == gameEvent.Game.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(temp => temp.CurrentMinute, gameEvent.Game.CurrentMinute)
                .SetProperty(temp => temp.TeamAScore, gameEvent.Game.TeamAScore)
                .SetProperty(temp => temp.TeamBScore, gameEvent.Game.TeamBScore)
            );

        if (gameEvent.EventType.IsTeamAScoredTry)
            gameEvent.Game.Teams[0].Team.ScoredTriesCount++;

        if (gameEvent.EventType.IsTeamBScoredTry)
            gameEvent.Game.Teams[1].Team.ScoredTriesCount++;

        if (gameEvent.EventType.IsTeamAScoring)
        {
            await db.Teams
                .Where(temp => temp.Id == gameEvent.Game.Teams[0].Team.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(temp => temp.PointsScored, 
                        temp => temp.PointsScored + gameEvent.EventType.GetTeamAScoreChange())
                    .SetPropertyIf(gameEvent.EventType.IsTeamAScoredTry,
                        temp => temp.ScoredTriesCount,
                        temp => temp.ScoredTriesCount + 1));
        }
        else if (gameEvent.EventType.IsTeamBScoring)
        {
            await db.Teams
                .Where(temp => temp.Id == gameEvent.Game.Teams[1].Team.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(temp => temp.PointsScored,
                        temp => temp.PointsScored + gameEvent.EventType.GetTeamBScoreChange())
                    .SetPropertyIf(gameEvent.EventType.IsTeamBScoredTry,
                        temp => temp.ScoredTriesCount,
                        temp => temp.ScoredTriesCount + 1));
        }
    }

    public GameEvent SimulateNextMinute(Game game)
    {
        game.CurrentMinute++;
        if (_stack is not null)
        {
            int count = _stack.Count;
            if (count == 0)
            {
                return new(game.CurrentMinute,
                    GameEventType.Nothing,
                    game.TeamAScore,
                    game.TeamBScore,
                    game);
            }

            int minutesLeft = 79 - game.CurrentMinute;
            int number = new Random().Next(1, minutesLeft + 1);
            if (number > count)
            {
                return new(game.CurrentMinute,
                    GameEventType.Nothing,
                    game.TeamAScore,
                    game.TeamBScore,
                    game);
            }

            var top = _stack.Pop();
            top.GameAction.Invoke(game);
            return new(game.CurrentMinute,
                top.EventType,
                game.TeamAScore,
                game.TeamBScore,
                game);
        }

        if (_teamAStats is null || _teamBStats is null)
        {
            _teamAStats = new(game.Teams[0]);
            _teamBStats = new(game.Teams[1]);
        }


        List<(GameEventType EventType, Action<Game> GameAction)> events = [];
        Random random = new();

        for (int teamATries = 0; teamATries < _teamAStats.GetTryCount(); teamATries++)
        {
            double successChance = _teamAStats.GetTrySuccessChance();
            double number = random.NextDouble();
            if (number <= successChance)
            {
                double conversionChance = _teamAStats.GetConversionSuccessChance();
                number = random.NextDouble();
                if (number <= conversionChance)
                {
                    events.Add((GameEventType.TeamAConvertedTry, gameChange =>
                    {
                        gameChange.TeamAScore += 7;
                    }
                    ));
                }
                else
                {
                    events.Add((GameEventType.TeamAUnconvertedTry, gameChange =>
                    {
                        gameChange.TeamAScore += 5;
                    }
                    ));
                }
            }
            else
            {
                events.Add((GameEventType.TeamAFailedTry, g => { }));
            }
        }

        for (int teamBTries = 0; teamBTries < _teamBStats.GetTryCount(); teamBTries++)
        {
            double successChance = _teamBStats.GetTrySuccessChance();
            double number = random.NextDouble();
            if (number <= successChance)
            {
                double conversionChance = _teamBStats.GetConversionSuccessChance();
                number = random.NextDouble();
                if (number <= conversionChance)
                {
                    events.Add((GameEventType.TeamBConvertedTry, gameChange =>
                    {
                        gameChange.TeamBScore += 7;
                    }
                    ));
                }
                else
                {
                    events.Add((GameEventType.TeamBUnconvertedTry, gameChange =>
                    {
                        gameChange.TeamBScore += 5;
                    }
                    ));
                }
            }
            else
            {
                events.Add((GameEventType.TeamBFailedTry, g => { }));
            }
        }

        for (int teamADropGoals = 0; teamADropGoals < _teamAStats.GetDropGoalCount(); teamADropGoals++)
        {
            double successChance = _teamAStats.GetDropGoalSuccessChance();
            double number = random.NextDouble();
            if (number <= successChance)
            {
                events.Add((GameEventType.TeamAScoredDropGoal, gameChange =>
                {
                    gameChange.TeamAScore += 3;
                }
                ));
            }
            else
            {
                events.Add((GameEventType.TeamAFailedDropGoal, g => { }));
            }
        }

        for (int teamBDropGoals = 0; teamBDropGoals < _teamBStats.GetDropGoalCount(); teamBDropGoals++)
        {
            double successChance = _teamBStats.GetDropGoalSuccessChance();
            double number = random.NextDouble();
            if (number <= successChance)
            {
                events.Add((GameEventType.TeamBScoredDropGoal, gameChange =>
                {
                    gameChange.TeamBScore += 3;
                }
                ));
            }
            else
            {
                events.Add((GameEventType.TeamBFailedDropGoal, g => { }));
            }
        }

        for (int teamAPenalties = 0; teamAPenalties < _teamBStats.GetOpponentPenaltyCount(); teamAPenalties++)
        {
            double successChance = _teamAStats.GetPenaltySuccessChance();
            double number = random.NextDouble();
            if (number <= successChance)
            {
                events.Add((GameEventType.TeamAScoredPenalty, gameChange =>
                {
                    gameChange.TeamAScore += 3;
                }
                ));
            }
            else
            {
                events.Add((GameEventType.TeamAMissedPenalty, g => { }));
            }
        }

        for (int teamBPenalties = 0; teamBPenalties < _teamAStats.GetOpponentPenaltyCount(); teamBPenalties++)
        {
            double successChance = _teamBStats.GetPenaltySuccessChance();
            double number = random.NextDouble();
            if (number <= successChance)
            {
                events.Add((GameEventType.TeamBScoredPenalty, gameChange =>
                {
                    gameChange.TeamBScore += 3;
                }
                ));
            }
            else
            {
                events.Add((GameEventType.TeamBMissedPenalty, g => { }));
            }
        }

        events.Sort((t1, t2) => random.Next(-100, 100));
        _stack = new(events);

        var stackTop = _stack.Pop();
        stackTop.GameAction.Invoke(game);

        GameEvent gameEvent = new(game.CurrentMinute,
            stackTop.EventType,
            game.TeamAScore,
            game.TeamBScore,
            game);

        return gameEvent;
    }


    public record Stats(int Insight, int Physique, int Technique)
    {
        public Stats(TeamGame team) : this(team.Team.Insight,
            team.Team.Physique,
            team.Team.Technique)
        {
            switch (team.Tactic)
            {
                case Tactics.General:
                    if (team.Coach == Coaches.General)
                    {
                        Insight = (int)Math.Floor(Insight * 1.12);
                        Physique = (int)Math.Floor(Physique * 1.12);
                        Technique = (int)Math.Floor(Technique * 1.12);
                    }
                    else
                    {
                        Insight = (int)Math.Floor(Insight * 1.08);
                        Physique = (int)Math.Floor(Physique * 1.08);
                        Technique = (int)Math.Floor(Technique * 1.08);
                    }
                    break;
                case Tactics.Insight:
                    if (team.Coach == Coaches.Insight) Insight += 10;
                    else Insight += 6;
                    break;
                case Tactics.Physique:
                    if (team.Coach == Coaches.Physique) Physique += 10;
                    else Physique += 6;
                    break;
                case Tactics.Technique:
                    if (team.Coach == Coaches.Technique) Technique += 10;
                    else Technique += 6;
                    break;
            }
        }

        public int GetTryCount()
            => (int)Math.Floor(((Physique * 0.7) + (Insight * 0.2) + (Technique * 0.1)) / 3);

        //returns the percentage
        public double GetTrySuccessChance()
        {
            double chance = ((Insight * 0.7) + (Technique * 0.3)) / 100.0;
            return chance < 1.0 ? chance : 1.0;
        }

        public double GetConversionSuccessChance()
        {
            double chance = ((Technique * 0.8) + (Insight * 0.2)) / 100.0;
            return chance < 1.0 ? chance : 1.0;
        }

        public int GetDropGoalCount()
            => (int)Math.Floor(((Insight * 0.8) + (Technique * 0.2)) / 10.0);

        public double GetDropGoalSuccessChance() => GetConversionSuccessChance();

        public int GetOpponentPenaltyCount()
            => (int)Math.Floor((50 - ((Insight * 0.8) + (Technique * 0.2))) / 4.0);

        public double GetPenaltySuccessChance() => GetConversionSuccessChance();
    };
}