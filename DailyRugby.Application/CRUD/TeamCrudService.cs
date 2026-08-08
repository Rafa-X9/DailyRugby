using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Application.CRUD;

public class TeamCrudService(AppDbContext db, ITeamValidatorFactory teamValidatorFactory)
    : ITeamCrudService
{
    public async Task<Result<TeamResponse>> AddAsync(TeamAddRequest? request)
    {
        if (request is null)
        {
            return Result<TeamResponse>.Failure("Null was passed as argument", Errors.NullArgument);
        }

        if (string.IsNullOrWhiteSpace(request.PlayerUsername))
        {
            return Result<TeamResponse>.Failure("Player's username can't be empty", Errors.Invalid);
        }

        if (request.Physique < 0 || request.Technique < 0 || request.Insight < 0)
        {
            return Result<TeamResponse>.Failure("No stat can be less than zero", Errors.Invalid);
        }

        var champ = await db.Championships
            .FirstOrDefaultAsync(temp => temp.Id == request.ChampionshipId);
        if (champ is null)
        {
            return Result<TeamResponse>.Failure("No such championship Id", Errors.NotFound);
        }

        var validator = teamValidatorFactory.GetValidatorForSeason(champ.Season);
        var validationResult = validator.Validate(request);
        if (!validationResult.IsSuccessful)
        {
            return Result<TeamResponse>.Failure(validationResult.Message, validationResult.Error);
        }

        Team team = request.ToTeam();
        db.Teams.Add(team);
        await db.SaveChangesAsync();
        return Result<TeamResponse>.Success(team.ToTeamResponse());
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        int affectedLines = await db.Teams
            .Where(temp => temp.Id == id)
            .ExecuteDeleteAsync();

        if (affectedLines < 1)
        {
            return Result.Failure("Team id wasn't found", Errors.NotFound);
        }

        return Result.Success();
    }

    public async Task<IList<TeamResponse>> GetAllAsync(Guid champId)
    {
        return (await db.Teams
            .Where(temp => temp.ChampionshipId == champId)
            .ToListAsync())
            .Select(temp => temp.ToTeamResponse())
            .ToList();
    }

    public async Task<Result<TeamResponse>> GetByIdAsync(Guid id)
    {
        Team? team = await db.Teams
            .FirstOrDefaultAsync(temp => temp.Id == id);

        if (team is null)
        {
            return Result<TeamResponse>.Failure("Given id wasn't found", Errors.NotFound);
        }

        return Result<TeamResponse>.Success(team.ToTeamResponse());
    }
}
