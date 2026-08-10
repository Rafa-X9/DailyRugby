namespace DailyRugby.Domain;

public record GameEvent(int Minute,
    GameEventType EventType,
    int TeamAScore,
    int TeamBScore);