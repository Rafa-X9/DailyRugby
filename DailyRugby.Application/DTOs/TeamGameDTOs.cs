using DailyRugby.Domain;

namespace DailyRugby.Application.DTOs;

public sealed record TeamGameResponse(Guid Id,
    TeamResponse Team,
    Coaches Coach,
    Tactics Tactic,
    CakeResponse? Cake,
    bool HasMoraleBoost,
    bool GetsMoraleBoostIfWins);

public static class TeamGameExtensions
{
    public static TeamGameResponse ToTeamGameResponse(this TeamGame teamGame)
        => new(teamGame.Id,
            teamGame.Team.ToTeamResponse(),
            teamGame.Coach,
            teamGame.Tactic,
            teamGame.Cake?.ToCakeResponse(),
            teamGame.HasMoraleBoost,
            teamGame.GetsMoraleBoostIfWins);
}