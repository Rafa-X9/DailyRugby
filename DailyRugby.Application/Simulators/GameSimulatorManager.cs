using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DailyRugby.Application.Simulators;

public class GameSimulatorManager(AppDbContext db) : BackgroundService, IGameSimulatorManager
{
    private readonly PriorityQueue<Schedule, DateTime> _schedules = new();

    public async Task<Result> ScheduleGameAsync(Guid gameId, DateTime dateTimeUtc)
    {
        if (dateTimeUtc <= DateTime.UtcNow)
        {
            return Result.Failure("Scheduled time must be in the future", Errors.Invalid);
        }

        var game = await db.Games
            .AsNoTracking()
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

        return Result.Success();
    }

    public async Task<IList<Schedule>> SeeScheduledGamesAsync(Guid champId, bool futureOnly = true)
        => await db.Schedules
            .AsNoTracking()
            .Include(temp => temp.Game)
            .Where(temp => temp.Game.ChampionshipId == champId
                && ((!futureOnly) || temp.DateTimeUtc > DateTime.UtcNow))
            .ToListAsync();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}