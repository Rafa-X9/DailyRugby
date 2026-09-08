namespace DailyRugby.Domain;

public class GameOdds
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public int TotalSimulations { get; set; }
    public int TeamAWins { get; set; }
    public int TeamBWins { get; set; }
    public int Tie => TotalSimulations - (TeamAWins + TeamBWins);
}