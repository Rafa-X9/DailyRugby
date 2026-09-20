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

    public async Task<Result<TeamResponse>> AddCoachAsync(Coaches coach, Guid teamId)
    {
        var team = await db.Teams.FirstOrDefaultAsync(temp => temp.Id == teamId);

        if (team is null)
        {
            return Result<TeamResponse>.Failure("Team id not found", Errors.NotFound);
        }

        switch (coach)
        {
            case Coaches.General:
                team.HasGeneralCoach = true;
                break;
            case Coaches.Insight:
                team.HasInsigthCoach = true;
                break;
            case Coaches.Physique:
                team.HasPhysiqueCoach = true;
                break;
            case Coaches.Technique:
                team.HasTechniqueCoach = true;
                break;
        }

        await db.SaveChangesAsync();

        return Result<TeamResponse>.Success(team.ToTeamResponse());
    }

    public async Task<Result<TeamResponse>> RemoveCoachAsync(Coaches coach, Guid teamId)
    {
        var team = await db.Teams.FirstOrDefaultAsync(temp => temp.Id == teamId);

        if (team is null)
        {
            return Result<TeamResponse>.Failure("Team id not found", Errors.NotFound);
        }

        switch (coach)
        {
            case Coaches.General:
                team.HasGeneralCoach = false;
                break;
            case Coaches.Insight:
                team.HasInsigthCoach = false;
                break;
            case Coaches.Physique:
                team.HasPhysiqueCoach = false;
                break;
            case Coaches.Technique:
                team.HasTechniqueCoach = false;
                break;
        }

        await db.SaveChangesAsync();

        return Result<TeamResponse>.Success(team.ToTeamResponse());
    }

    public async Task<Result<TeamResponse>> AddToStatAsync(int amount, TeamStats stat, Guid teamId)
    {
        var team = await db.Teams.FirstOrDefaultAsync(temp => temp.Id == teamId);

        if (team is null)
        {
            return Result<TeamResponse>.Failure("Team id not found", Errors.NotFound);
        }

        switch(stat)
        {
            case TeamStats.Insight:
                team.Insight += amount;
                break;
            case TeamStats.Physique:
                team.Physique += amount;
                break;
            case TeamStats.Technique:
                team.Technique += amount;
                break;
        };

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

    public async Task<IList<TeamResponse>> GetAllAsync()
    {
        return (await db.Teams
            .ToListAsync())
            .Select(temp => temp.ToTeamResponse())
            .ToList();
    }

    public async Task<IList<TeamResponse>> GetAllAsync(Guid champId)
    {
        return await db.Teams
            .AsNoTracking()
            .Include(temp => temp.Cakes.Where(cake => !cake.IsUsed))
            .Where(temp => temp.ChampionshipId == champId)
            .Select(temp => temp.ToTeamResponse())
            .ToListAsync();
    }

    public async Task<Result<TeamResponse>> GetByIdAsync(Guid id)
    {
        Team? team = await db.Teams
            .AsNoTracking()
            .Include(temp => temp.Cakes.Where(cake => !cake.IsUsed))
            .FirstOrDefaultAsync(temp => temp.Id == id);

        if (team is null)
        {
            return Result<TeamResponse>.Failure("Given id wasn't found", Errors.NotFound);
        }

        return Result<TeamResponse>.Success(team.ToTeamResponse());
    }

    public async Task<Result<TeamResponse>> AddCakeAsync(Guid teamId, string cakeName, int amount = 1)
    {
        if (cakeName.Length <= 0 || cakeName.Length > 100)
        {
            return Result<TeamResponse>.Failure("Cake name must be between 1-100 characters",
                Errors.Invalid);
        }

        var team = await db.Teams
            .Include(temp => temp.Cakes)
            .FirstOrDefaultAsync(temp => temp.Id == teamId);

        if (team is null)
        {
            return Result<TeamResponse>.Failure("Id not found", Errors.NotFound);
        }

        for (int i = 0; i < amount; i++)
        {
            Cake cake = new()
            {
                Id = Guid.CreateVersion7(),
                Name = cakeName,
                TeamId = team.Id
            };
            team.Cakes.Add(cake);
        }

        await db.SaveChangesAsync();

        return Result<TeamResponse>.Success(team.ToTeamResponse());
    }

    public async Task<IList<CakeResponse>> GetCakesFromTeamAsync(Guid teamId)
        => await db.Cakes
        .AsNoTracking()
        .Where(temp => temp.TeamId == teamId && !temp.IsUsed)
        .Select(temp => temp.ToCakeResponse())
        .ToListAsync();
}
