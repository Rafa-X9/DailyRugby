namespace DailyRugby.Domain;

public class TeamGame
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid GameId { get; set; }
    public required Team Team { get; set; }
    public Coaches Coach { get; set; } = Coaches.None;
    public Tactics Tactic { get; set; } = Tactics.None;
    public bool IsUsingCake { get; set; }
    public bool HasMoraleBoost { get; set; }
    public bool GetsMoraleBoostIfWins { get; set; }
}