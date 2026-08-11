using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DailyRugby.Application.Simulators;

public class GameSimulatorManager(IServiceProvider serviceProvider) : BackgroundService, IGameSimulatorManager
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
                .AsNoTracking()
                .Include(temp => temp.Teams)
                .ThenInclude(temp => temp.Team)
                .FirstOrDefaultAsync(temp => temp.Id == gameId);


            if (game is null)
            {
                return Result.Failure("Game Id not found", Errors.NotFound);
            }

            Schedule schedule = new()
            {
                DateTimeUtc = dateTimeUtc,
                GameId = game.Id
            };

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
            if (earliestGame.DateTimeUtc >= DateTime.UtcNow)
            {
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                GameEvent gameEvent = new(0, GameEventType.GameStarted, 0, 0, earliestGame.Game);
                GameEventHappened?.Invoke(this, gameEvent);
                _schedules.Dequeue();
                await db.Schedules
                    .Where(temp => temp.Id == earliestGame.Id)
                    .ExecuteDeleteAsync();
            }
        }
    }

    private async Task WaitDelay()
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}