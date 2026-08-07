using DailyRugby.Domain;

namespace DailyRugby.Application.DTOs;

public sealed record TeamGameResponse(Guid Id,
    TeamResponse Team,
    Coaches Coach,
    bool IsUsingCake,
    bool HasMoraleBoost,
    bool GetsMoraleBoostIfWins);

public static class TeamGameExtensions
{
    public static TeamGameResponse ToTeamGameResponse(this TeamGame teamGame)
        => new(teamGame.Id,
            teamGame.Team.ToTeamResponse(),
            teamGame.Coach,
            teamGame.IsUsingCake,
            teamGame.HasMoraleBoost,
            teamGame.GetsMoraleBoostIfWins);
}