using DailyRugby.Domain;

namespace DailyRugby.Application.Utilitaries;

public static class GameEventTypeHelpers
{
    public static bool IsTeamAScoredTry(this GameEventType type)
        => type.IsIn(GameEventType.TeamAUnconvertedTry, GameEventType.TeamAConvertedTry);

    public static bool IsTeamBScoredTry(this GameEventType type)
        => type.IsIn(GameEventType.TeamBUnconvertedTry, GameEventType.TeamBConvertedTry);
}