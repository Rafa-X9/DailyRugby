using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.CRUD;

public class TeamCrudService(AppDbContext db, ITeamValidatorFactory teamValidatorFactory)
    : ITeamCrudService
{
    public Task<Result<TeamResponse>> AddAsync(TeamAddRequest? request)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IList<TeamResponse>> GetAllAsync(Guid champId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ChampionshipResponse>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
