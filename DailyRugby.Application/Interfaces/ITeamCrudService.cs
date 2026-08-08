using DailyRugby.Application.DTOs;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface ITeamCrudService
{
    Task<Result<TeamResponse>> AddAsync(TeamAddRequest? request);

    Task<IList<TeamResponse>> GetAllAsync();

    Task<IList<TeamResponse>> GetAllAsync(Guid champId);

    Task<Result<TeamResponse>> GetByIdAsync(Guid id);

    Task<Result> DeleteAsync(Guid id);
}