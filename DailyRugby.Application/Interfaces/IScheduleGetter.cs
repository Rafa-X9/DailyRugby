using DailyRugby.Application.DTOs;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IScheduleGetter
{
    Task<Result<IList<ScheduleResponse>>> GetSchedulesAsync();
}