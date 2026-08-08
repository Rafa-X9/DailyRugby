using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Shared;

namespace DailyRugby.Application.CRUD;

public class ChampionshipCrudService : IChampionshipCrudService
{
    public Task<Result<ChampionshipResponse>> AddAsync(ChampionshipAddRequest? request)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IList<ChampionshipResponse>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<ChampionshipResponse>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}