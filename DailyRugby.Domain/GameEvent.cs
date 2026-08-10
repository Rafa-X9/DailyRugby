namespace DailyRugby.Domain;

public class GameEvent(int minute,
    GameEventType eventType,
    int teamAScore,
    int teamBScore,
    Game game) : EventArgs
{
    public int Minute { get; set; } = minute;
    public GameEventType EventType { get; set; } = eventType;
    public int TeamAScore { get; set; } = teamAScore;
    public int TeamBScore { get; set; } = teamBScore;
    public Game Game { get; set; } = game;
};