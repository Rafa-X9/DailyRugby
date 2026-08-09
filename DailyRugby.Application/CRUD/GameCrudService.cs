using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Shared;
using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Application.CRUD;

public class GameCrudService(AppDbContext db) : IGameCrudService
{
    public async Task<Result<IList<GameResponse>>> GenerateRounds(Guid champId)
    {
        var champ = await db.Championships
            .Include(temp => temp.Teams)
            .FirstOrDefaultAsync(temp => temp.Id == champId);

        if (champ is null)
        {
            return Result<IList<GameResponse>>
                .Failure("Championship Id not found", Errors.NotFound);
        }

        if (champ.Teams.Count < 2)
        {
            return Result<IList<GameResponse>>
                .Failure("Championship must have at least 2 teams", Errors.Invalid);
        }

        List<Game> games = [];

        var teamIds = champ.Teams.Select(temp => temp.Id).ToList();
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

            teamGames.Add(new TeamGame
            {
                TeamId = homeTeam.Id,
                GameId = game.Id
            });

            teamGames.Add(new TeamGame
            {
                TeamId = awayTeam.Id,
                GameId = game.Id
            });
        }

        db.TeamGames.AddRange(teamGames);
        await db.SaveChangesAsync();

        var gamesWithTeams = (await db.Games
            .AsNoTracking()
            .Where(temp => temp.ChampionshipId == champId)
            .Include(temp => temp.Teams)
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

    public Task<IList<GameResponse>> GetAllAsync(Guid champId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<GameResponse>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IList<GameResponse>> GetByTeamIdAsync(Guid champId)
    {
        throw new NotImplementedException();
    }
}