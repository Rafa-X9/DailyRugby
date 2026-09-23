using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DailyRugby.Domain;

public class ChampionshipOdds
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [ForeignKey(nameof(Championship))]
    public Guid ChampionshipId { get; set; }
    public Championship Championship { get; set; } = null!;
    public int CompletedGamesCount { get; set; }

    public string FirstPlaceOddsJson { get; set; } = null!;

    public string LastPlaceOddsJson { get; set; } = null!;

    [NotMapped]
    public List<TeamChampOdds> FirstPlaceOdds
        => JsonSerializer.Deserialize<List<TeamChampOdds>>(FirstPlaceOddsJson)!;

    [NotMapped]
    public List<TeamChampOdds> LastPlaceOdds
        => JsonSerializer.Deserialize<List<TeamChampOdds>>(LastPlaceOddsJson)!;
}

public sealed record TeamChampOdds(string Country, double Chance);