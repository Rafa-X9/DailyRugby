using DailyRugby.Domain;

namespace DailyRugby.Application.DTOs;

public sealed record GameResponse(Guid Id,
    Guid ChampId,
    int TeamAScore,
    int TeamBScore,
    TeamGameResponse TeamA,
    TeamGameResponse TeamB,
    int Round,
    int CurrentMinute,
    GameState CurrentState);

public static class GameExtensions
{
    public static GameResponse ToGameResponse(this Game game)
        => new(game.Id,
            game.ChampionshipId,
            game.TeamAScore,
            game.TeamBScore,
            game.Teams[0].ToTeamGameResponse(),
            game.Teams[1].ToTeamGameResponse(),
            game.Round,
            game.CurrentMinute,
            game.CurrentState);
}