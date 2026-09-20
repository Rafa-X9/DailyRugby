using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Shared;

namespace DailyRugby.Application.CRUD;

public class JsonGetter(IGameCrudService gameService,
    IChampionshipCrudService champService,
    ITeamCrudService teamService) : IJsonGetter
{
    public async Task<Result<object>> GetCurrentRoundAsync()
    {
        var result = await gameService.GetCurrentRoundAsync();

        if (!result.IsSuccessful)
        {
            return Result<object>.Failure(result.Message, result.Error);
        }

        var games = new List<object>();

        foreach (var game in result.Item)
        {
            games.Add(new
            {
                teamA = game.TeamA.Team.Country,
                teamB = game.TeamB.Team.Country,

                teamAHasMoraleBoost = game.TeamA.HasMoraleBoost,
                teamBHasMoraleBoost = game.TeamB.HasMoraleBoost,

                teamAGetsMoraleBoost = game.TeamA.GetsMoraleBoostIfWins,
                teamBGetsMoraleBoost = game.TeamB.GetsMoraleBoostIfWins
            });
        }

        return Result<object>.Success(games);
    }

    public async Task<Result<object>> GetLeaderBoardAsync(Guid champId)
    {
        var champResult = await champService.GetByIdAsync(champId);
        if (!champResult.IsSuccessful)
        {
            return Result<object>.Failure(champResult.Message, champResult.Error);
        }

        var teams = await champService.GetStandingsAsync(champId);

        List<object> list = [];

        int roundCount = champResult.Item.Games.Max(game => game.Round);

        foreach (var pair in teams)
        {
            TeamResponse team = pair.Value;
            int gamesPlayedCount = team.WinCount + team.TieCount + team.LossCount;

            List<string> teamsBeaten = [];
            var gamesPlayed = champResult.Item.Games
                .Where(game => game.TeamA.Team.Id == team.Id
                    || game.TeamB.Team.Id == team.Id);

            foreach (var gamePlayed in gamesPlayed)
            {
                if (gamePlayed.TeamA.Team.Id == team.Id
                    && gamePlayed.TeamAScore > gamePlayed.TeamBScore)
                {
                    teamsBeaten.Add(gamePlayed.TeamB.Team.Country);
                }
                else if (gamePlayed.TeamB.Team.Id == team.Id
                    && gamePlayed.TeamBScore > gamePlayed.TeamAScore)
                {
                    teamsBeaten.Add(gamePlayed.TeamA.Team.Country);
                }
            }

            list.Add(new
            {
                country = team.Country,
                wins = team.WinCount,
                ties = team.TieCount,
                losses = team.LossCount,
                hasBeaten = teamsBeaten,
                pointBalance = team.PointsScored - team.PointsTaken,
                tryBalance = team.ScoredTriesCount - team.SufferedTriesCount,
                tries = team.ScoredTriesCount,
                points = team.PointsScored,
                matchesLeft = roundCount - gamesPlayedCount
            });
        }

        return Result<object>.Success(list);
    }

    public async Task<Result<object>> GetPreviousRoundAsync()
    {
        var currentRoundResult = await gameService.GetCurrentRoundAsync();

        if (!currentRoundResult.IsSuccessful)
        {
            return Result<object>.Failure(currentRoundResult.Message, currentRoundResult.Error);
        }

        if (currentRoundResult.Item.Count == 0)
        {
            return Result<object>.Failure("There is no previous round", Errors.Invalid);
        }

        Guid champId = currentRoundResult.Item[0].ChampId;
        int round = currentRoundResult.Item[0].Round - 1;

        var result = await gameService.GetRoundAsync(champId, round);

        if (!result.IsSuccessful)
        {
            return Result<object>.Failure(result.Message, result.Error);
        }

        var games = new List<object>();

        foreach (var game in result.Item)
        {
            games.Add(new
            {
                teamA = game.TeamA.Team.Country,
                teamB = game.TeamB.Team.Country,

                teamAScore = game.TeamAScore,
                teamBScore = game.TeamBScore,

                teamATactic = game.TeamA.Tactic.ToString(),
                teamBTactic = game.TeamB.Tactic.ToString(),

                teamAUsedCake = game.TeamA.Cake is not null,
                teamBUsedCake = game.TeamB.Cake is not null,

                teamAHadMoraleBoost = game.TeamA.HasMoraleBoost,
                teamBHadMoraleBoost = game.TeamB.HasMoraleBoost,

                teamAGotMoraleBoost = game.TeamA.GetsMoraleBoostIfWins,
                teamBGotMoraleBoost = game.TeamB.GetsMoraleBoostIfWins
            });
        }

        return Result<object>.Success(games);
    }

    public async Task<Result<object>> GetSchedulesAsync(Guid champId)
    {
        var champResult = await champService.GetByIdAsync(champId);

        if (!champResult.IsSuccessful)
        {
            return Result<object>.Failure(champResult.Message, champResult.Error);
        }

        Dictionary<int, List<string>> rounds = [];

        foreach (var game in champResult.Item.Games)
        {
            if (rounds.TryGetValue(game.Round, out List<string>? list) && list is not null)
            {
                list.Add($"{game.TeamA.Team.Country} vs {game.TeamB.Team.Country}");
            }
            else
            {
                rounds[game.Round] = [$"{game.TeamA.Team.Country} vs {game.TeamB.Team.Country}"];
            }
        }

        return Result<object>.Success(rounds);
    }

    public async Task<Result<object>> GetTeamsAsync(Guid champId)
    {
        var teams = await teamService.GetAllAsync(champId);

        var list = new List<object>();

        foreach (var team in teams)
        {
            list.Add(new
            {
                country = team.Country,
                username = team.PlayerUsername,
                technique = team.Technique,
                insight = team.Insight,
                physique = team.Physique,
                coaches = team.Coaches.Select(temp => temp.ToString()),
                cakes = team.Cakes.Select(temp => temp.Name)
            });
        }

        return Result<object>.Success(list);
    }
}
