using DailyRugby.Application.DTOs;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface ITeamCrudService
{
    Task<Result<TeamResponse>> AddAsync(TeamAddRequest request);

    Task<IList<TeamResponse>> GetAllAsync();

    Task<Result<ChampionshipResponse>> GetByIdAsync(Guid id);

    Task<Result> DeleteAsync(Guid id);
}