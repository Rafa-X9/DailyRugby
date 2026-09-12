using DailyRugby.Application.DTOs;
using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface ITeamCrudService
{
    Task<Result<TeamResponse>> AddAsync(TeamAddRequest? request);

    Task<IList<TeamResponse>> GetAllAsync();

    Task<IList<TeamResponse>> GetAllAsync(Guid champId);

    Task<Result<TeamResponse>> GetByIdAsync(Guid id);

    Task<Result> DeleteAsync(Guid id);

    Task<Result<TeamResponse>> AddToStatAsync(int amount, TeamStats stat, Guid teamId);

    Task<Result<TeamResponse>> AddCoachAsync(Coaches coach, Guid teamId);
}