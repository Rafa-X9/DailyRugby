namespace DailyRugby.Domain;

public enum GameEventType
{
    Nothing,
    
    GameStarted,

    TeamAFailedTry,
    TeamAUnconvertedTry,
    TeamAConvertedTry,
    
    TeamBFailedTry,
    TeamBUnconvertedTry,
    TeamBConvertedTry
}