using DailyRugby.Application.DTOs;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IGameCrudService
{
    Task<Result<GameResponse>> AddGameAsync();

    Task<IList<GameResponse>> GetAllAsync();

    Task<Result<GameResponse>> GetByIdAsync(Guid id);

    Task<Result> DeleteAsync(Guid id);
}