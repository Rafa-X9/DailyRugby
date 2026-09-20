using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Utilitaries;
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
    private ISpecificGameSimulator? _currentSimulator = null;
    private Game? _ongoingGame = null;

    public Result AddCheer(CheerAddRequest request)
    {
        if (_currentSimulator is null || _ongoingGame is null)
        {
            return Result.Failure("There isn't an ongoing game", Errors.Invalid);
        }

        Cheer cheer = new()
        {
            Id = Guid.NewGuid(),
            ForTeamA = request.ForTeamA,
            Yell = request.Yell ?? string.Empty,
            UserId = request.UserId
        };

        return _currentSimulator.AddCheer(cheer, _ongoingGame);
    }

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

            await db.Schedules
                .Where(temp => temp.GameId == gameId)
                .ExecuteDeleteAsync();

            game = await db.Games
                //.AsNoTracking()
                .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
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
                    .ThenInclude(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
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
                Game game;

                using (var scope = serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    _schedules.Dequeue();
                    await db.Schedules
                        .Where(temp => temp.Id == earliestGame.Id)
                        .ExecuteDeleteAsync();

                    game = await db.Games
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(temp => temp.Teams.OrderBy(t => t.Team.Country))
                            .ThenInclude(temp => temp.Team)
                        .Include(temp => temp.Championship)
                        .Where(temp => temp.Id == earliestGame.Game.Id)
                        .FirstAsync(stoppingToken);
                }
                await SimulateGameAsync(game);
            }
        }
    }

    private async Task SimulateGameAsync(Game game)
    {
        _ongoingGame = game;

        if (game.CurrentState == GameState.Scheduled)
        {
            using var startScope = serviceProvider.CreateScope();
            var startDb = startScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await startDb.Games
                .Where(temp => temp.Id == game.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(temp => temp.CurrentState, GameState.Started));


            GameEvent started = new(-1, GameEventType.GameStarted,
                0,
                0,
                game);
            GameEventHappened?.Invoke(this, started);
        }

        var simulator = new GameSimulatorFactory()
            .GetGameSimulator(game.Championship.Season);
        _currentSimulator = simulator;

        while (game.CurrentMinute < 80)
        {
            GameEvent gameEvent = simulator.SimulateNextMinute(game);

            if (gameEvent.EventType.RequiresOwnMinute)
            {
                await timer.WaitUntilNextMinuteAsync();
            }
            else
            {
                await timer.WaitFifteenSecondsAsync();
            }

            using (var eventScope = serviceProvider.CreateScope())
            {
                var eventDb = eventScope.ServiceProvider.GetRequiredService<AppDbContext>();
                await simulator.SaveGameAsync(gameEvent, eventDb);
            }

            GameEventHappened?.Invoke(this, gameEvent);

            if (gameEvent.EventType.RequiresOwnMinute)
            {
                game.CurrentMinute++;
            }

            if (game.CurrentMinute == 40)
            {
                await timer.WaitUntilNextMinuteAsync();
                GameEvent halfTime = new(40,
                    GameEventType.HalfTime,
                    game.TeamAScore,
                    game.TeamBScore,
                    game);
                GameEventHappened?.Invoke(this, halfTime);
                await timer.WaitFifteenMinutesAsync();
                continue;
            }
        }

        Team winner, loser;

        if (game.TeamAScore >= game.TeamBScore)
        {
            winner = game.Teams[0].Team;
            loser = game.Teams[1].Team;
        }
        else
        {
            winner = game.Teams[1].Team;
            loser = game.Teams[0].Team;
        }

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Games
            .Where(temp => temp.Id == game.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(temp => temp.CurrentState, GameState.Finished));

        if (game.TeamAScore != game.TeamBScore)
        {
            await db.Teams
                .Where(temp => temp.Id == winner.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(temp => temp.WinCount, temp => temp.WinCount + 1));

            await db.Teams
                .Where(temp => temp.Id == loser.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(temp => temp.LossCount, temp => temp.LossCount + 1));
        }
        else
        {
            await db.Teams
                .Where(temp => temp.Id == winner.Id || temp.Id == loser.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(temp => temp.TieCount, temp => temp.TieCount + 1));
        }

        GameEvent finished = new(game.CurrentMinute,
            GameEventType.GameFinished,
            game.TeamAScore,
            game.TeamBScore,
            game);
        GameEventHappened?.Invoke(this, finished);

        _ongoingGame = null;
        _currentSimulator = null;
    }

    private async Task WaitDelay()
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}