using DailyRugby.Domain;

namespace DailyRugby.Application.DTOs;

public sealed record GameAddRequest(Guid TeamAId,
    Guid TeamBId,
    DateTime ScheduledTime);

public sealed record GameResponse(Guid Id,
    TeamGameResponse TeamA,
    TeamGameResponse TeamB,
    DateTime ScheduledTime,
    int CurrentMinute,
    GameState CurrentState);

public static class GameExtensions
{
    public static Game ToGame(this GameAddRequest request)
        => new()
        {
            TeamAId = request.TeamAId,
            TeamBId = request.TeamBId,
            ScheduledTime = request.ScheduledTime
        };

    public static GameResponse ToGameResponse(this Game game)
        => new(game.Id,
            game.TeamA.ToTeamGameResponse(),
            game.TeamB.ToTeamGameResponse(),
            game.ScheduledTime,
            game.CurrentMinute,
            game.CurrentState);
}