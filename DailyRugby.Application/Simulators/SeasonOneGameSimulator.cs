using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DailyRugby.Application.Simulators;

public class SeasonOneGameSimulator(IServiceProvider serviceProvider) : ISpecificGameSimulator
{
    public async Task<GameEvent> SimulateNextMinute(Game game)
    {
        game.CurrentMinute++;

        GameEvent gameEvent = new(game.CurrentMinute,
            GameEventType.TeamAConvertedTry,
            69,
            0,
            game);

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Games
            .Where(temp => temp.Id == game.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(temp => temp.CurrentMinute, game.CurrentMinute)
            );

        return gameEvent;
    }
}