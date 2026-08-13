using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DailyRugby.Application.Simulators;

public class GameSimulatorManager(IServiceProvider serviceProvider,
    IGameTimer timer)
    : BackgroundService, IGameSimulatorManager
{
    private readonly PriorityQueue<Schedule, DateTime> _schedules = new();
    public event EventHandler? GameEventHappened;

    public async Task<Result> ScheduleGameAsync(Guid gameId, DateTime dateTimeUtc)
    {
        if (dateTimeUtc <= DateTime.UtcNow)
        {
            return Result.Failure("Scheduled time must be in the future", Errors.Invalid);
        }

        Game? game;

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            game = await db.Games
                //.AsNoTracking()
                .Include(temp => temp.Teams)
                    .ThenInclude(temp => temp.Team)
                .Include(temp => temp.Championship)
                .FirstOrDefaultAsync(temp => temp.Id == gameId);


            if (game is null)
            {
                return Result.Failure("Game Id not found", Errors.NotFound);
            }

            Schedule schedule = new()
            {
                DateTimeUtc = dateTimeUtc,
                GameId = game.Id,
                Game = game
            };
            game.CurrentState = GameState.Scheduled;

            db.Schedules.Add(schedule);

            await db.SaveChangesAsync();

            _schedules.Enqueue(schedule, schedule.DateTimeUtc);
        }

        return Result.Success();
    }

    public async Task<IList<Schedule>> SeeScheduledGamesAsync(Guid champId, bool futureOnly = true)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Schedules
            .AsNoTracking()
            .Include(temp => temp.Game)
            .Where(temp => temp.Game.ChampionshipId == champId
                && ((!futureOnly) || temp.DateTimeUtc > DateTime.UtcNow))
            .ToListAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            List<Schedule> schedules = await db.Schedules
                .AsNoTracking()
                .Include(temp => temp.Game)
                    .ThenInclude(temp => temp.Teams)
                        .ThenInclude(temp => temp.Team)
                .Include(temp => temp.Game)
                    .ThenInclude(temp => temp.Championship)
                .AsSplitQuery()
                .ToListAsync(stoppingToken);

            foreach (var schedule in schedules)
            {
                _schedules.Enqueue(schedule, schedule.DateTimeUtc);
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_schedules.Count == 0)
            {
                await WaitDelay();
                continue;
            }

            var earliestGame = _schedules.Peek();
            if (earliestGame.DateTimeUtc <= DateTime.UtcNow)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    _schedules.Dequeue();
                    await db.Schedules
                        .Where(temp => temp.Id == earliestGame.Id)
                        .ExecuteDeleteAsync();
                }
                await SimulateGameAsync(earliestGame.Game);
            }
        }
    }

    private async Task SimulateGameAsync(Game game)
    {
        if (game.CurrentState == GameState.Scheduled)
        {
            using var startScope = serviceProvider.CreateScope();
            var startDb = startScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await startDb.Games
                .Where(temp => temp.Id == game.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(temp => temp.CurrentState, GameState.Started));


            GameEvent started = new(0, GameEventType.GameStarted,
                -1,
                -1,
                game);
            GameEventHappened?.Invoke(this, started);
        }

        var simulator = new GameSimulatorFactory(serviceProvider)
            .GetGameSimulator(game.Championship.Season);
        while (game.CurrentMinute <= 80)
        {
            GameEvent gameEvent = await simulator.SimulateNextMinute(game);
            GameEventHappened?.Invoke(this, gameEvent);
            if (game.CurrentMinute == 40)
            {
                GameEvent halfTime = new(40,
                    GameEventType.HalfTime,
                    -1 /*FIX*/,
                    -1 /*FIX*/,
                    game);
                GameEventHappened?.Invoke(this, halfTime);
                await timer.WaitFifteenMinutesAsync();
            }
            else
            {
                await timer.WaitOneMinuteAsync();
            }
        }

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Games
            .Where(temp => temp.Id == game.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(temp => temp.CurrentState, GameState.Finished));

        GameEvent finished = new(game.CurrentMinute,
            GameEventType.GameFinished,
            -1,
            -1,
            game);
        GameEventHappened?.Invoke(this, finished);
    }

    private async Task WaitDelay()
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}