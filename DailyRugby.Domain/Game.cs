using System.ComponentModel.DataAnnotations.Schema;

namespace DailyRugby.Domain;

public class Game
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public TeamGame TeamA { get; set; }
    public TeamGame TeamB { get; set; }
    public int Round { get; set; }
    public DateTime ScheduledTime { get; set; }
    public int CurrentMinute { get; set; }
    public GameState CurrentState { get; set; } = GameState.Scheduled;
    
    [ForeignKey(nameof(TeamA))]
    public Guid TeamAId { get; set; }

    [ForeignKey(nameof(TeamB))]
    public Guid TeamBId { get; set; }
}