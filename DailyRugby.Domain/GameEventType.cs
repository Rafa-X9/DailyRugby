namespace DailyRugby.Domain;

public enum GameEventType
{
    Nothing,
    
    GameStarted,
    HalfTime,
    GameFinished,

    TeamAFailedTry,
    TeamAUnconvertedTry,
    TeamAConvertedTry,
    
    TeamBFailedTry,
    TeamBUnconvertedTry,
    TeamBConvertedTry
}