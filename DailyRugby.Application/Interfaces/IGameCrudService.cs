using DailyRugby.Application.DTOs;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface IGameCrudService
{
    Task<Result<IList<GameResponse>>> GenerateRounds(Guid champId);

    Task<IList<GameResponse>> GetAllAsync(Guid champId);

    Task<IList<GameResponse>> GetByTeamIdAsync(Guid champId);

    Task<Result<GameResponse>> GetByIdAsync(Guid id);
}