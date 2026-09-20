using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Application.CRUD;

public class GameCrudService(AppDbContext db) : IGameCrudService
{

    public async Task<Result<IList<GameResponse>>> GenerateRounds(Guid champId, bool overwriteIfExists = false)
    {
        var champ = await db.Championships
            .Include(temp => temp.Teams)
            .FirstOrDefaultAsync(temp => temp.Id == champId);

        if (champ is null)
        {
            return Result<IList<GameResponse>>
                .Failure("Championship Id not found", Errors.NotFound);
        }

        if (champ.State != ChampionshipState.NotStarted)
        {
            if (overwriteIfExists)
            {
                await db.Games
                    .Where(temp => temp.ChampionshipId == champId)
                    .ExecuteDeleteAsync();
                champ.State = ChampionshipState.NotStarted;
                await db.SaveChangesAsync();
                return await GenerateRounds(champId, false);
            }
            else
            {
                return Result<IList<GameResponse>>.Failure("Can't generate rounds of an " +
                    "already started championship", Errors.Invalid);
            }
        }

        if (champ.Teams.Count < 2)
        {
            return Result<IList<GameResponse>>
                .Failure("Championship must have at least 2 teams", Errors.Invalid);
        }

        

        List<Game> games = [];

        var teamIds = champ.Teams.Select(temp => temp.Id).ToList();
        Random random = new();
        teamIds.Sort((id1, id2) => random.Next(-10, 10));
        if (teamIds.Count % 2 == 1)
        {
            teamIds.Add(Guid.Empty);
        }

        int count = teamIds.Count;
        int rounds = count - 1;

        List<(Game game, Guid homeTeamId, Guid awayTeamId)> pairings = [];

        for (int round = 0; round < rounds; round++)
        {
            for (int i = 0; i < count / 2; i++)
            {
                var homeId = teamIds[i];
                var awayId = teamIds[count - 1 - i];

                if (homeId == Guid.Empty || awayId == Guid.Empty) continue;

                var game = new Game
                {
                    ChampionshipId = champId,
                    Championship = champ,
                    Round = round + 1
                };

                games.Add(game);
                db.Games.Add(game);

                pairings.Add((game, homeId, awayId));
            }

            var last = teamIds[count - 1];
            teamIds.RemoveAt(count - 1);
            teamIds.Insert(1, last);
        }

        await db.SaveChangesAsync();

        var teamGames = new List<TeamGame>();
        foreach (var (game, homeId, awayId) in pairings)
        {
            var homeTeam = champ.Teams.First(t => t.Id == homeId);
            var awayTeam = champ.Teams.First(t => t.Id == awayId);

            Coaches homeCoach;
            if (homeTeam.HasGeneralCoach) homeCoach = Coaches.General;
            else if (homeTeam.HasInsigthCoach) homeCoach = Coaches.Insight;
            else if (homeTeam.HasPhysiqueCoach) homeCoach = Coaches.Physique;
            else if (homeTeam.HasTechniqueCoach) homeCoach = Coaches.Technique;
            else homeCoach = Coaches.None;

            Coaches awayCoach;
            if (awayTeam.HasGeneralCoach) awayCoach = Coaches.General;
            else if (awayTeam.HasInsigthCoach) awayCoach = Coaches.Insight;
            else if (awayTeam.HasPhysiqueCoach) awayCoach = Coaches.Physique;
            else if (awayTeam.HasTechniqueCoach) awayCoach = Coaches.Technique;
            else awayCoach = Coaches.None;

            teamGames.Add(new TeamGame
            {
                TeamId = homeTeam.Id,
                GameId = game.Id,
                Coach = homeCoach
            });

            teamGames.Add(new TeamGame
            {
                TeamId = awayTeam.Id,
                GameId = game.Id,
                Coach = awayCoach
            });
        }

        db.TeamGames.AddRange(teamGames);
        champ.State = ChampionshipState.Started;
        db.Championships.Update(champ);
        await db.SaveChangesAsync();

        var gamesWithTeams = (await db.Games
            .AsNoTracking()
            .Where(temp => temp.ChampionshipId == champId)
            .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
                .ThenInclude(temp => temp.Team)
            .ToListAsync())
            .Select(temp =>
            {
                temp.Championship = champ;
                return temp.ToGameResponse();
            })
            .ToList();

        return Result<IList<GameResponse>>.Success(gamesWithTeams);
    }

    public async Task<IList<GameResponse>> GetAllAsync(Guid champId)
    {
        return (await db.Games
            .Where(temp => temp.ChampionshipId == champId)
            .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
                .ThenInclude(temp => temp.Team)
            .ToListAsync())
            .Select(temp => temp.ToGameResponse())
            .ToList();
    }

    public async Task<IList<GameResponse>> GetAllAsync()
    {
        return (await db.Games
            .AsNoTracking()
            .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
                .ThenInclude(temp => temp.Team)
            .ToListAsync())
            .Select(temp => temp.ToGameResponse())
            .ToList();
    }

    public async Task<Result<GameResponse>> GetByIdAsync(Guid id)
    {
        var game = await db.Games
            .AsNoTracking()
            .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
                .ThenInclude(temp => temp.Team)
            .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
                .ThenInclude(temp => temp.Cake)
            .FirstOrDefaultAsync(temp => temp.Id == id);

        if (game is null)
        {
            return Result<GameResponse>.Failure("Id not found", Errors.NotFound);
        }

        return Result<GameResponse>.Success(game.ToGameResponse());
    }

    public async Task<Result<IList<GameResponse>>> GetByTeamIdAsync(Guid teamId)
    {
        var team = await db.Teams.FirstOrDefaultAsync(temp => temp.Id == teamId);

        if (team is null)
        {
            return Result<IList<GameResponse>>.Failure("No such team Id", Errors.NotFound);
        }

        var games = (await db.Games
            .AsNoTracking()
            .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
            .ThenInclude(temp => temp.Team)
            .Where(temp => temp.Teams.Any(t => t.Team.Id == team.Id))
            .ToListAsync())
            .Select(temp => temp.ToGameResponse())
            .ToList();

        return Result<IList<GameResponse>>.Success(games);
    }

    public async Task<Result<IList<GameResponse>>> GetCurrentRoundAsync()
    {
        var games = await db.Games
            .AsNoTracking()
            .Include(temp => temp.Championship)
            .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
            .ThenInclude(temp => temp.Team)
            .Where(temp => temp.Championship.IsMainChampionship)
            .ToListAsync();
        
        if (games.Count == 0)
        {
            return Result<IList<GameResponse>>.Failure("There isn't a main championship ongoing, " +
                "or it has no rounds",
                Errors.Invalid);
        }

        int roundCount = games.Max(temp => temp.Round);
        for (int i = 1; i <= roundCount + 1; i++)
        {
            var round = games.Where(temp => temp.Round == i);

            if (round.Any(temp => temp.CurrentState != GameState.Finished))
            {
                return Result<IList<GameResponse>>
                    .Success(round.Select(temp => temp.ToGameResponse()).ToList());
            }
        }

        return Result<IList<GameResponse>>.Failure("There isn't an ongoing round", Errors.Invalid);
    }

    public async Task<Result<IList<GameResponse>>> GetRoundAsync(Guid champId, int round)
    {
        var games = await db.Games
            .AsNoTracking()
            .Include(temp => temp.Championship)
            .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
            .ThenInclude(temp => temp.Team)
            .Where(temp => temp.ChampionshipId == champId
                && temp.Championship.IsMainChampionship
                && temp.Round == round)
            .Select(temp => temp.ToGameResponse())
            .ToListAsync();

        return Result<IList<GameResponse>>.Success(games);
    }

    public async Task<Result<TeamGameResponse>> SetCakeAsync(Guid gameId, Teams team, string cake)
    {
        var game = await db.Games
            .Include(temp => temp.Teams.OrderBy(t => t.Team.Country))
                .ThenInclude(temp => temp.Team)
                .ThenInclude(temp => temp.Cakes)
            .Include(temp => temp.Teams.OrderBy(t => t.Team.Country))
                .ThenInclude(temp => temp.Cake)
            .FirstOrDefaultAsync(temp => temp.Id == gameId);

        if (game is null)
        {
            return Result<TeamGameResponse>.Failure("Id not found", Errors.NotFound);
        }

        TeamGame teamGame = team == Teams.TeamA ? game.Teams[0] : game.Teams[1];

        var cakeToUse = teamGame.Team.Cakes
            .FirstOrDefault(temp => temp.Name.Equals(cake, StringComparison.OrdinalIgnoreCase));

        if (cakeToUse is null)
        {
            return Result<TeamGameResponse>.Failure("Cake not found. The cakes " +
                $"{teamGame.Team.Country} has are: {string.Join(", ", teamGame.Team.Cakes
                .Select(temp => temp.Name))}", Errors.NotFound);
        }

        teamGame.Cake = cakeToUse;
        await db.SaveChangesAsync();
        return Result<TeamGameResponse>.Success(teamGame.ToTeamGameResponse());
    }

    public async Task<Result<TeamGameResponse>> SetCoachAsync(Guid gameId, Teams team, Coaches coach)
    {
        var game = await db.Games
            .Include(temp => temp.Teams.OrderBy(t => t.Team.Country))
                .ThenInclude(temp => temp.Team)
            .FirstOrDefaultAsync(temp => temp.Id == gameId);

        if (game is null)
        {
            return Result<TeamGameResponse>.Failure("Id not found", Errors.NotFound);
        }

        TeamGame teamGame = team == Teams.TeamA ? game.Teams[0] : game.Teams[1];

        bool hasCoach = coach switch
        {
            Coaches.General => teamGame.Team.HasGeneralCoach,
            Coaches.Insight => teamGame.Team.HasInsigthCoach,
            Coaches.Physique => teamGame.Team.HasPhysiqueCoach,
            Coaches.Technique => teamGame.Team.HasTechniqueCoach,
            _ => false
        };

        if (!hasCoach)
        {
            return Result<TeamGameResponse>.Failure($"{teamGame.Team.Country} does not " +
                $"have the {coach} coach", Errors.Invalid);
        }

        teamGame.Coach = coach;
        await db.SaveChangesAsync();
        return Result<TeamGameResponse>.Success(teamGame.ToTeamGameResponse());
    }

    public async Task<Result<TeamGameResponse>> SetTacticAsync(Guid gameId, Teams team, Tactics tactic)
    {
        var game = await db.Games
            .Include(temp => temp.Teams.OrderBy(temp => temp.Team.Country))
                .ThenInclude(temp => temp.Team)
            .FirstOrDefaultAsync(temp => temp.Id == gameId);

        if (game is null)
        {
            return Result<TeamGameResponse>.Failure("Given Id wasn't found", Errors.NotFound);
        }

        TeamGameResponse response;

        if (team == Teams.TeamA)
        {
            game.Teams[0].Tactic = tactic;
            response = game.Teams[0].ToTeamGameResponse();
        }
        else
        {
            game.Teams[1].Tactic = tactic;
            response = game.Teams[1].ToTeamGameResponse();
        }

        await db.SaveChangesAsync();
        return Result<TeamGameResponse>.Success(response);
    }
}