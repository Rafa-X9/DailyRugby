using DailyRugby.Shared;
using DailyRugby.Application.DTOs;

namespace DailyRugby.Application.Interfaces;

public interface IChampionshipCrudService
{
    Task<Result<ChampionshipResponse>> AddAsync(ChampionshipAddRequest? request);

    Task<IList<ChampionshipResponse>> GetAllAsync();

    Task<Result<ChampionshipResponse>> GetByIdAsync(Guid id);

    Task<Result<ChampionshipResponse>> SetAsMainAsync(Guid id);

    Task<Result<ChampionshipResponse>> UnsetAsMainAsync(Guid id);

    Task<Result> DeleteAsync(Guid id);
}