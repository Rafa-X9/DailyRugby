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
    private GameOdds _result = new();

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

        _result = new() { Id = Guid.NewGuid() };

        if (game is null) return Result<GameOdds>.Failure("Id not found", Errors.NotFound);

        _game = game;
        _result.GameId = gameId;

        _factory = serviceProvider.GetRequiredService<IGameSimulatorFactory>();

        await Task.Run(Simulate);

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GameOdds.Add(_result);
            await db.SaveChangesAsync();
        }

        return Result<GameOdds>.Success(_result);
    }

    private void Simulate()
    {
        for (int repetition = 0; repetition < _repetitions; repetition++)
        {
            _game.CurrentMinute = -1;
            _game.CurrentState = GameState.Started;
            _game.TeamAScore = 0;
            _game.TeamBScore = 0;

            var simulator = _factory.GetGameSimulator(_game.Championship.Season);

            for (int minute = 0; minute < 80; minute++)
            {
                simulator.SimulateNextMinute(_game);
            }

            _result.TotalSimulations++;

            if (_game.TeamAScore > _game.TeamBScore)
            {
                _result.TeamAWins++;
            }
            else if (_game.TeamAScore < _game.TeamBScore)
            {
                _result.TeamBWins++;
            }
        }
    }
}