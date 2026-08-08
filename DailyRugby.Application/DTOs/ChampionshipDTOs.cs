using DailyRugby.Domain;

namespace DailyRugby.Application.DTOs;

public sealed record ChampionshipAddRequest(string Name);

public sealed record ChampionshipResponse(Guid Id,
    string Name,
    ChampionshipState State,
    IReadOnlyList<TeamResponse> Teams,
    IReadOnlyList<GameResponse> Games);

public static class ChampionshipExtensions
{
    public static Championship ToChampionship(this ChampionshipAddRequest request)
        => new()
        {
            Name = request.Name
        };

    public static ChampionshipResponse ToChampionshipResponse(this Championship champ)
        => new(champ.Id,
            champ.Name,
            champ.State,
            champ.Teams.Select(team => team.ToTeamResponse()).ToList().AsReadOnly(),
            champ.Games.Select(game => game.ToGameResponse()).ToList().AsReadOnly());
}