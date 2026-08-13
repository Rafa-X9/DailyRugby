using System.ComponentModel.DataAnnotations.Schema;

namespace DailyRugby.Domain;

public class Game
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public IList<TeamGame> Teams { get; set; } = [];
    public Championship Championship { get; set; }
    public int Round { get; set; }
    public int CurrentMinute { get; set; }
    public GameState CurrentState { get; set; }
    public int TeamAScore { get; set; }
    public int TeamBScore { get; set; }

    [ForeignKey(nameof(Championship))]
    public Guid ChampionshipId { get; set; }
}