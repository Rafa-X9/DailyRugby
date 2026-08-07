using DailyRugby.Domain;
using Microsoft.EntityFrameworkCore;

namespace DailyRugby.Application.DTOs;

public sealed record TeamAddRequest(Guid ChampionshipId,
    string PlayerUsername,
    int Insight,
    int Physique,
    int Technique,
    Coaches InitialCoach);

public sealed record TeamResponse(Guid Id,
    Guid ChampionshipId,
    string PlayerUsername,
    int Insight,
    int Physique,
    int Technique,
    List<Coaches> Coaches,
    int CakesAmount);

public static class TeamExtensions
{
    public static Team ToTeam(this TeamAddRequest request)
    {
        Team team = new()
        {
            ChampionshipId = request.ChampionshipId,
            PlayerUsername = request.PlayerUsername,
            Insight = request.Insight,
            Physique = request.Physique,
            Technique = request.Technique
        };

        if (request.InitialCoach == Coaches.Insight)
            team.HasInsigthCoach = true;
        else if (request.InitialCoach == Coaches.Technique)
            team.HasTechniqueCoach = true;
        else if (request.InitialCoach == Coaches.Physique)
            team.HasPhysiqueCoach = true;
        else if (request.InitialCoach == Coaches.General)
            team.HasGeneralCoach = true;

        return team;
    }

    public static TeamResponse ToTeamResponse(this Team team)
    {
        TeamResponse response = new(team.Id,
            team.ChampionshipId,
            team.PlayerUsername,
            team.Insight,
            team.Physique,
            team.Technique,
            [],
            team.CakesAmount);

        if (team.HasGeneralCoach) response.Coaches.Add(Coaches.General);
        if (team.HasInsigthCoach) response.Coaches.Add(Coaches.Insight);
        if (team.HasPhysiqueCoach) response.Coaches.Add(Coaches.Physique);
        if (team.HasTechniqueCoach) response.Coaches.Add(Coaches.Technique);

        return response;
    }
}