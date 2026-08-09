using DailyRugby.Domain;

namespace DailyRugby.Application.DTOs;

public sealed record GameResponse(Guid Id,
    TeamGameResponse TeamA,
    TeamGameResponse TeamB,
    DateTime ScheduledTime,
    int Round,
    int CurrentMinute,
    GameState CurrentState);

public static class GameExtensions
{
    public static GameResponse ToGameResponse(this Game game)
        => new(game.Id,
            game.Teams[0].ToTeamGameResponse(),
            game.Teams[1].ToTeamGameResponse(),
            game.ScheduledTime,
            game.Round,
            game.CurrentMinute,
            game.CurrentState);
}