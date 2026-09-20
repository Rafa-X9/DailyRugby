using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IJsonGetter
{
    Task<Result<object>> GetSchedulesAsync(Guid champId);
    Task<Result<object>> GetLeaderBoardAsync(Guid champId);
    Task<Result<object>> GetTeamsAsync(Guid champId);
    Task<Result<object>> GetPreviousRoundAsync();
    Task<Result<object>> GetCurrentRoundAsync();
}