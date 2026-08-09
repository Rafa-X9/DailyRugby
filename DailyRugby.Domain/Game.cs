using System.ComponentModel.DataAnnotations.Schema;

namespace DailyRugby.Domain;

public class Game
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public IList<TeamGame> Teams { get; set; } = [];
    public Championship Championship { get; set; }
    public int Round { get; set; }
    public DateTime ScheduledTime { get; set; }
    public int CurrentMinute { get; set; }
    public GameState CurrentState { get; set; }
    

    [ForeignKey(nameof(Championship))]
    public Guid ChampionshipId { get; set; }
}