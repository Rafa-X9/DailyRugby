namespace DailyRugby.Domain;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required TeamGame TeamA { get; set; }
    public required TeamGame TeamB { get; set; }
    public DateTime ScheduledTime { get; set; }
    public int CurrentMinute { get; set; }
    public GameState CurrentState { get; set; } = GameState.Scheduled;
    public Guid TeamAId { get; set; }
    public Guid TeamBId { get; set; }
}