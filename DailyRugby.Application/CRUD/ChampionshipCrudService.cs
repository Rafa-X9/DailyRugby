using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;

namespace DailyRugby.Application.CRUD;

public class ChampionshipCrudService(AppDbContext db) : IChampionshipCrudService
{
    public async Task<Result<ChampionshipResponse>> AddAsync(ChampionshipAddRequest? request)
    {
        if (request is null)
        {
            return Result<ChampionshipResponse>.Failure("Null was passed as argument", Errors.NullArgument);
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<ChampionshipResponse>.Failure("Name can't be empty", Errors.Invalid);
        }

        Championship champ = request.ToChampionship();
        db.Championships.Add(champ);
        await db.SaveChangesAsync();
        var response = champ.ToChampionshipResponse();
        return Result<ChampionshipResponse>.Success(response);
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