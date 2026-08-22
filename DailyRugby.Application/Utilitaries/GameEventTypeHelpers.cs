using DailyRugby.Domain;

namespace DailyRugby.Application.Utilitaries;

public static class GameEventTypeHelpers
{
    extension(GameEventType type)
    {
        public bool IsTeamAScoredTry
            => type.IsIn(GameEventType.TeamAUnconvertedTry, GameEventType.TeamAConvertedTry);

        public bool IsTeamBScoredTry
            => type.IsIn(GameEventType.TeamBUnconvertedTry, GameEventType.TeamBConvertedTry);

        public bool IsTeamAScoring
            => type.IsIn(GameEventType.TeamAUnconvertedTry,
                GameEventType.TeamAConvertedTry,
                GameEventType.TeamAScoredDropGoal,
                GameEventType.TeamAScoredPenalty);

        public bool IsTeamBScoring
            => type.IsIn(GameEventType.TeamBUnconvertedTry,
                GameEventType.TeamBConvertedTry,
                GameEventType.TeamBScoredDropGoal,
                GameEventType.TeamBScoredPenalty);

        public int GetTeamAScoreChange()
        {
            if (type.IsTeamAScoredTry)
            {
                return type == GameEventType.TeamAUnconvertedTry ? 5 : 7;
            }
            return type.IsTeamAScoring ? 3 : 0;
        }

        public int GetTeamBScoreChange()
        {
            if (type.IsTeamBScoredTry)
            {
                return type == GameEventType.TeamBUnconvertedTry ? 5 : 7;
            }
            return type.IsTeamBScoring ? 3 : 0;
        }
    }
}