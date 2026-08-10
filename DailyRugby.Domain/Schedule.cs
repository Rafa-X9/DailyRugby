namespace DailyRugby.Domain;

public class Schedule
{
    public Guid Id { get; set; }
    public DateTime DateTimeUtc { get; set; }
    public Guid GameId { get; set; }
    public Game Game { get; set; }
}