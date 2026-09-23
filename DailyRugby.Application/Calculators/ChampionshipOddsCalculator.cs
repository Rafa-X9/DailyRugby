using DailyRugby.Application.CRUD;
using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace DailyRugby.Application.Calculators;

public class ChampionshipOddsCalculator(IServiceProvider serviceProvider) : IChampionshipOddsCalculator
{
    private readonly Random _random = new();
    private const int _repetitions = 10_000;

    public async Task<Result<ChampionshipOdds>> GetOddsAsync(Guid champId, bool passIfNotExists = false)
    {
        Championship championship;
        List<GameOdds> gameOdds;

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var champOdds = await db.ChampionshipOdds
                .AsNoTracking()
                .Include(temp => temp.Championship)
                    .ThenInclude(temp => temp.Games)
                        .ThenInclude(temp => temp.Teams.OrderBy(t => t.Team.Country))
                            .ThenInclude(temp => temp.Team)
                .Include(temp => temp.Championship)
                    .ThenInclude(temp => temp.Games)
                        .ThenInclude(temp => temp.Teams.OrderBy(t => t.Team.Country))
                            .ThenInclude(temp => temp.Cake)
                .Include(temp => temp.Championship)
                    .ThenInclude(temp => temp.Teams)
                .FirstOrDefaultAsync(temp => temp.ChampionshipId == champId);

            if (champOdds is null && passIfNotExists)
            {
                return Result<ChampionshipOdds>.Failure("Championship odds calculation are not " +
                    "yet finished. Please wait until it's finished.", Errors.Invalid);
            }

            if (champOdds is not null && champOdds.CompletedGamesCount == champOdds.Championship
                .Games.Count(temp => temp.CurrentState == GameState.Finished))
            {
                return Result<ChampionshipOdds>.Success(champOdds);
            }
            else if (passIfNotExists)
            {
                return Result<ChampionshipOdds>.Failure("Championship odds exists " +
                    "but is outdated. Please wait until it's refreshed.", Errors.Invalid);
            }

            if (champOdds is not null) championship = champOdds.Championship;
            else
            {
                var champ = await db.Championships
                    .AsNoTracking()
                        .Include(temp => temp.Games)
                            .ThenInclude(temp => temp.Teams.OrderBy(t => t.Team.Country))
                                .ThenInclude(temp => temp.Team)
                        .Include(temp => temp.Games)
                            .ThenInclude(temp => temp.Teams.OrderBy(t => t.Team.Country))
                                .ThenInclude(temp => temp.Cake)
                        .Include(temp => temp.Teams)
                    .FirstOrDefaultAsync(temp => temp.Id == champId);

                if (champ is null)
                {
                    return Result<ChampionshipOdds>.Failure("Championship Id not found",
                        Errors.NotFound);
                }

                championship = champ;
            }

            var gameIds = championship.Games
                .Where(temp => temp.CurrentState != GameState.Finished)
                .Select(temp => temp.Id)
                .ToList();

            gameOdds = await db.GameOdds
                .AsNoTracking()
                .Where(temp => gameIds.Contains(temp.GameId))
                .ToListAsync();

            if (gameOdds.Count != gameIds.Count)
            {
                return Result<ChampionshipOdds>.Failure("All games' odds must have been " +
                    "finished to get championship's odds", Errors.Invalid);
            }
        }

        Dictionary<string, int> firstPlaces = [];
        Dictionary<string, int> lastPlaces = [];

        for (int i = 0; i < _repetitions; i++)
        {
            (string first, string last) = Simulate(championship, gameOdds);

            if (firstPlaces.ContainsKey(first))
                firstPlaces[first]++;
            else
                firstPlaces[first] = 1;

            if (lastPlaces.ContainsKey(last))
                lastPlaces[last]++;
            else
                lastPlaces[last] = 1;
        }

        List<TeamChampOdds> firstPlaceChances = [];
        foreach (var pair in firstPlaces)
        {
            firstPlaceChances.Add(new(pair.Key, PercentageOf(pair.Value, _repetitions)));
        }

        List<TeamChampOdds> lastPlaceChances = [];
        foreach (var pair in lastPlaces)
        {
            lastPlaceChances.Add(new(pair.Key, PercentageOf(pair.Value, _repetitions)));
        }

        ChampionshipOdds odds = new()
        {
            ChampionshipId = championship.Id,
            Id = Guid.CreateVersion7(),
            FirstPlaceOddsJson = JsonSerializer.Serialize(firstPlaceChances),
            LastPlaceOddsJson = JsonSerializer.Serialize(lastPlaceChances)
        };

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ChampionshipOdds.Add(odds);
            await db.SaveChangesAsync();
        }

        return Result<ChampionshipOdds>.Success(odds);
    }

    private (string first, string last) Simulate(Championship championship, List<GameOdds> gameOdds)
    {
        var champCopy = new Championship()
        {
            Name = championship.Name,
            Season = championship.Season,
            State = championship.State,
            IsMainChampionship = championship.IsMainChampionship,
            Games = championship.Games
                .Select(game => game.CurrentState == GameState.Finished ? game : new Game()
                {
                    Id = game.Id,
                    CurrentMinute = game.CurrentMinute,
                    CurrentState = game.CurrentState,
                    Round = game.Round,
                    TeamAScore = 0,
                    TeamBScore = 0,
                    Teams = game.Teams
                        .Select(teamGame => new TeamGame()
                        {
                            Cake = teamGame.Cake,
                            Coach = teamGame.Coach,
                            Tactic = teamGame.Tactic,
                            Players = teamGame.Players.Select(player => new Player(player.Id,
                                player.Number,
                                player.Insight,
                                player.Physique,
                                player.Technique,
                                player.IsOnField))
                                .ToList(),
                            Team = new Team()
                            {
                                Id = teamGame.Team.Id,
                                Technique = teamGame.Team.Technique,
                                Physique = teamGame.Team.Physique,
                                Insight = teamGame.Team.Insight
                            },
                            HasMoraleBoost = teamGame.HasMoraleBoost,
                            Id = teamGame.Id
                        })
                        .ToList()
                })
                .ToList(),
            Teams = championship.Teams
                .Select(team => new Team()
                {
                    Id = team.Id,
                    WinCount = team.WinCount,
                    TieCount = team.TieCount,
                    LossCount = team.LossCount,
                    PointsScored = team.PointsScored,
                    Country = team.Country,
                    PointsTaken = team.PointsTaken,
                    ScoredTriesCount = team.ScoredTriesCount,
                    SufferedTriesCount = team.SufferedTriesCount
                })
                .ToList()
        };

        var gamesToSimulate = champCopy.Games
            .Where(temp => temp.CurrentState != GameState.Finished);

        foreach (var game in gamesToSimulate)
        {
            var odds = gameOdds.First(temp => temp.GameId == game.Id);

            int roll = _random.Next(1, odds.TotalSimulations + 1);

            if (roll <= odds.TeamAWins)
            {
                champCopy.Teams
                    .First(temp => temp.Id == game.Teams[0].Team.Id)
                    .WinCount++;
                champCopy.Teams
                    .First(temp => temp.Id == game.Teams[1].Team.Id)
                    .LossCount++;
            }
            else if (roll <= odds.TeamAWins + odds.Tie)
            {
                champCopy.Teams
                    .First(temp => temp.Id == game.Teams[0].Team.Id)
                    .TieCount++;
                champCopy.Teams
                    .First(temp => temp.Id == game.Teams[1].Team.Id)
                    .TieCount++;
            }
            else
            {
                champCopy.Teams
                    .First(temp => temp.Id == game.Teams[1].Team.Id)
                    .WinCount++;
                champCopy.Teams
                    .First(temp => temp.Id == game.Teams[0].Team.Id)
                    .LossCount++;
            }
        }

        List<Team> ordered = champCopy.Teams
            .OrderByDescending(temp => temp.WinCount)
            .ThenByDescending(temp => temp.GetPointsAgainstTiedTeams(champCopy.Teams, champCopy.Games))
            .ThenByDescending(temp => temp.PointsScored - temp.PointsTaken)
            .ThenByDescending(temp => temp.ScoredTriesCount - temp.SufferedTriesCount)
            .ThenByDescending(temp => temp.ScoredTriesCount)
            .ThenByDescending(temp => temp.PointsScored)
            .ThenBy(temp => _random.Next())
            .ToList();

        return (ordered.First().Country, ordered.Last().Country);
    }

    private static double PercentageOf(int number, int total)
    {
        return (double)number / total;
    }
}