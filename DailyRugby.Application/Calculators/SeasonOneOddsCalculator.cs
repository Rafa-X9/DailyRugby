using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DailyRugby.Application.Calculators;

public class SeasonOneOddsCalculator(IServiceProvider serviceProvider)
    : IGameOddsCalculator
{
    private const int _repetitions = 5_000;
    private Game _game = null!;
    private ISpecificGameSimulator _simulator = null!;
    private GameOdds _result = new() { Id = Guid.CreateVersion7() };

    public async Task CalculateAsync(Guid gameId)
    {
        Game? game;
        using (var scope = serviceProvider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            game = await db.Games
                .AsNoTracking()
                .Include(temp => temp.Championship)
                .Include(temp => temp.Teams)
                    .ThenInclude(temp => temp.Team)
                .FirstOrDefaultAsync(temp => temp.Id == gameId);
        }

        if (game is null) return;

        _game = game;
        _result.GameId = gameId;

        var simulatorFactory = serviceProvider.GetRequiredService<IGameSimulatorFactory>();
        _simulator = simulatorFactory.GetGameSimulator(game.Championship.Season);

        await Task.Run(Simulate);

        using (var scope = serviceProvider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GameOdds.Add(_result);
            await db.SaveChangesAsync();
        }
    }

    private void Simulate()
    {
        for (int repetition = 0; repetition < _repetitions; repetition++)
        {
            Game copy = new()
            {
                CurrentMinute = _game.CurrentMinute,
                CurrentState = _game.CurrentState,
                TeamAScore = 0,
                TeamBScore = 0,
                Teams = _game.Teams
            };

            for (int minute = 0; minute < 80; minute++)
            {
                _simulator.SimulateNextMinute(copy);
            }

            _result.TotalSimulations++;

            if (copy.TeamAScore > copy.TeamBScore)
            {
                _result.TeamAWins++;
            }
            else if (copy.TeamAScore < copy.TeamBScore)
            {
                _result.TeamBWins++;
            }
        }
    }
}