using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Result> DeleteAsync(Guid id)
    {
        int linesAffected = await db.Championships
            .Where(temp => temp.Id == id)
            .ExecuteDeleteAsync();

        if (linesAffected < 1)
        {
            return Result.Failure("Given id wasn't found", Errors.NotFound);
        }

        return Result.Success();
    }

    public async Task<IList<ChampionshipResponse>> GetAllAsync()
    {
        return (await db.Championships
            .ToListAsync())
            .Select(temp => temp.ToChampionshipResponse())
            .ToList();
    }

    public async Task<Result<ChampionshipResponse>> GetByIdAsync(Guid id)
    {
        var match = await db.Championships.FirstOrDefaultAsync(temp => temp.Id == id);

        if (match is null)
        {
            return Result<ChampionshipResponse>.Failure("Given id wasn't found", Errors.NotFound);
        }

        return Result<ChampionshipResponse>.Success(match.ToChampionshipResponse());
    }

    public async Task<SortedDictionary<int, TeamResponse>> GetStandingsAsync(Guid id)
    {
        var champ = await db.Championships
            .AsNoTracking()
            .Include(temp => temp.Teams)
            .FirstOrDefaultAsync(temp => temp.Id == id);

        if (champ is null) return [];

        List<Team> ordered = champ.Teams
            .OrderByDescending(temp => temp.WinCount)
            .ThenByDescending(temp => temp.GetPointsAgainstTiedTeams(champ.Teams, champ.Games))
            .ThenByDescending(temp => temp.PointsScored - temp.PointsTaken)
            .ThenByDescending(temp => temp.ScoredTriesCount - temp.SufferedTriesCount)
            .ThenByDescending(temp => temp.ScoredTriesCount)
            .ThenByDescending(temp => temp.PointsScored)
            .ThenBy(temp => new Random().Next())
            .ToList();

        SortedDictionary<int, TeamResponse> response = [];

        for (int i = 0; i < ordered.Count; i++)
        {
            response[i + 1] = ordered[i].ToTeamResponse();
        }

        return response;
    }

    public async Task<Result<ChampionshipResponse>> SetAsMainAsync(Guid id)
    {
        if (await db.Championships.AnyAsync(temp => temp.IsMainChampionship))
        {
            return Result<ChampionshipResponse>.Failure("There is already a main championship",
                Errors.Invalid);
        }

        var champ = await db.Championships.FirstOrDefaultAsync(temp => temp.Id == id);
        if (champ is null)
        {
            return Result<ChampionshipResponse>.Failure("No such championship Id",
                Errors.NotFound);
        }

        champ.IsMainChampionship = true;
        await db.SaveChangesAsync();

        return Result<ChampionshipResponse>.Success(champ.ToChampionshipResponse());
    }

    public async Task<Result<ChampionshipResponse>> UnsetAsMainAsync(Guid id)
    {
        var champ = await db.Championships.FirstOrDefaultAsync(temp => temp.Id == id);
        
        if (champ is null)
        {
            return Result<ChampionshipResponse>.Failure("Given id wasn't found",
                Errors.NotFound);
        }

        champ.IsMainChampionship = false;
        await db.SaveChangesAsync();
        return Result<ChampionshipResponse>.Success(champ.ToChampionshipResponse());
    }
}

public static partial class TeamExtensions
{
    public static int GetPointsAgainstTiedTeams(this Team team,
        IList<Team> others,
        IList<Game> games)
    {
        var tiedTeams = others
            .Where(temp => temp.WinCount == team.WinCount)
            .ToList();

        if (tiedTeams.Count == 0) return 0;

        int wins = 0;

        var gamesWithTiedTeams = games
            .Where(temp => temp.Teams.Any(t => t.Team.Id == team.Id))
            .Where(temp => temp.Teams.Any(t1 => tiedTeams.Any(t2 => t1.Team.Id == t2.Id)));

        foreach (var game in gamesWithTiedTeams)
        {
            if (game.Teams[0].Team.Id == team.Id
                && game.TeamAScore > game.TeamBScore)
            {
                wins++;
            }
            else if (game.Teams[1].Team.Id == team.Id
                && game.TeamBScore > game.TeamAScore)
            {
                wins++;
            }
        }

        return wins;
    }
}