using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DailyRugby.Domain;

public class TeamGame
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TeamId { get; set; }
    [ForeignKey(nameof(Game))]
    public Guid GameId { get; set; }
    public Game Game { get; set; }
    public Team Team { get; set; }
    public Coaches Coach { get; set; } = Coaches.None;
    public Tactics Tactic { get; set; } = Tactics.None;
    public bool IsUsingCake { get; set; }
    public bool HasMoraleBoost { get; set; }
    public bool GetsMoraleBoostIfWins { get; set; }
}