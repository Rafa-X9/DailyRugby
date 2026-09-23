using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DailyRugby.Application.Calculators;

public class GameOddsCalculator(IServiceProvider serviceProvider)
    : IGameOddsCalculator
{
    private const int _repetitions = 5_000;
    private Game _game = null!;
    private IGameSimulatorFactory _factory = null!;

    public async Task<Result<GameOdds>> GetOddsAsync(Guid gameId, bool passIfNotExists = false)
    {
        Game? game;
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var odds = await db.GameOdds.FirstOrDefaultAsync(temp => temp.GameId == gameId);

            if (odds is not null) return Result<GameOdds>.Success(odds);

            game = await db.Games
                .AsNoTracking()
                .Include(temp => temp.Championship)
                .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
                    .ThenInclude(temp => temp.Team)
                .FirstOrDefaultAsync(temp => temp.Id == gameId);
        }

        if (passIfNotExists)
        {
            return Result<GameOdds>.Failure("Calculation is not yet finished, please wait",
                Errors.Invalid);
        }

        GameOdds result = new() { Id = Guid.NewGuid() };

        if (game is null) return Result<GameOdds>.Failure("Id not found", Errors.NotFound);

        _game = game;
        result.GameId = gameId;

        _game.Teams[0].Tactic = Tactics.None;
        _game.Teams[1].Tactic = Tactics.None;

        _factory = serviceProvider.GetRequiredService<IGameSimulatorFactory>();

        List<GameResult> simulationResults = [];
        for (int i = 0; i < _repetitions; i++)
        {
            await Task.Run(() => simulationResults.Add(Simulate()));
        }

        result.TotalSimulations = simulationResults.Count;
        foreach (var gameResult in simulationResults)
        {
            if (gameResult == GameResult.TeamAWon) result.TeamAWins++;
            else if (gameResult == GameResult.TeamBWon) result.TeamBWins++;
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GameOdds.Add(result);
            await db.SaveChangesAsync();
        }

        return Result<GameOdds>.Success(result);
    }

    private GameResult Simulate()
    {
        Game clone = new()
        {
            CurrentMinute = -1,
            CurrentState = GameState.Started,
            TeamAScore = 0,
            TeamBScore = 0,
            Teams = _game.Teams
        };

        var simulator = _factory.GetGameSimulator(_game.Championship.Season);

        for (int minute = 0; minute < 80; minute++)
        {
            simulator.SimulateNextMinute(clone);
        }

        if (clone.TeamAScore > clone.TeamBScore)
        {
            return GameResult.TeamAWon;
        }
        else if (clone.TeamAScore < clone.TeamBScore)
        {
            return GameResult.TeamBWon;
        }

        return GameResult.Tie;
    }

    private enum GameResult
    {
        TeamAWon, TeamBWon, Tie
    }
}