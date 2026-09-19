using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Application.CRUD;

public class ScheduleGetter(AppDbContext db, IGameCrudService gameService)
    : IScheduleGetter
{
    public async Task<Result<IList<ScheduleResponse>>> GetSchedulesAsync()
    {
        var roundResult = await gameService.GetCurrentRoundAsync();

        if (!roundResult.IsSuccessful)
        {
            return Result<IList<ScheduleResponse>>.Failure(roundResult.Message,
                roundResult.Error);
        }

        var schedules = await db.Schedules
            .AsNoTracking()
            .Include(temp => temp.Game)
                .ThenInclude(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
                .ThenInclude(temp => temp.Team)
            .Where(temp => temp.Game.Round == roundResult.Item[0].Round)
            .Select(temp => new ScheduleResponse(temp.DateTimeUtc,
                temp.Game.Teams[0].Team.Country,
                temp.Game.Teams[1].Team.Country))
            .ToListAsync();

        return Result<IList<ScheduleResponse>>.Success(schedules);
    }
}