namespace DailyRugby.Domain;

public class Cheer
{
    public Guid Id { get; set; }
    public ulong UserId { get; set; }
    public bool ForTeamA { get; set; }
    public string Yell { get; set; } = string.Empty;
    public int StartMinute { get; set; }
}