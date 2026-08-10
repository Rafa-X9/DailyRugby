namespace DailyRugby.Domain;

public enum GameEventType
{
    Nothing,

    TeamAFailedTry,
    TeamAUnconvertedTry,
    TeamAConvertedTry,
    
    TeamBFailedTry,
    TeamBUnconvertedTry,
    TeamBConvertedTry
}