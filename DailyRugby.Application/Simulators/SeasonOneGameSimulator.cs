using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DailyRugby.Application.Simulators;

public class SeasonOneGameSimulator(IServiceProvider serviceProvider) : ISpecificGameSimulator
{
    private Stats? _teamAStats;
    private Stats? _teamBStats;
    private Stack<(GameEventType EventType, Action<Game> GameAction)>? _stack;

    public async Task<GameEvent> SimulateNextMinute(Game game)
    {
        game.CurrentMinute++;
        if (_stack is not null)
        {
            if (_stack.Count == 0)
            {
                return new(game.CurrentMinute,
                    GameEventType.Nothing,
                    game.TeamAScore,
                    game.TeamBScore,
                    game);
            }

            var top = _stack.Pop();
            top.GameAction.Invoke(game);
            await SaveGame(game);
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
            if (successChance <= number)
            {
                double conversionChance = _teamAStats.GetConversionSuccessChance();
                number = random.NextDouble();
                if (conversionChance <= number)
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
            if (successChance <= number)
            {
                double conversionChance = _teamBStats.GetConversionSuccessChance();
                number = random.NextDouble();
                if (conversionChance <= number)
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
            if (successChance <= number)
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
            if (successChance <= number)
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
            if (successChance <= number)
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
            if (successChance <= number)
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

        await SaveGame(game);

        return gameEvent;
    }


    private record Stats(int Insight, int Physique, int Technique)
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
            double chance = (((Insight * 0.7) + (Technique * 0.7)) / 3.0) / 100.0;
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

        public double GetOpponentPenaltyCount()
            => (int)Math.Floor((50 - ((Insight * 0.8) + (Technique * 0.2))) / 4.0);

        public double GetPenaltySuccessChance() => GetConversionSuccessChance();
    };

    private async Task SaveGame(Game game)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Games
            .Where(temp => temp.Id == game.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(temp => temp.CurrentMinute, game.CurrentMinute)
                .SetProperty(temp => temp.TeamAScore, game.TeamAScore)
                .SetProperty(temp => temp.TeamBScore, game.TeamBScore)
            );
    }
}