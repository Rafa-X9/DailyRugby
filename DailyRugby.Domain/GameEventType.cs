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
    TeamBConvertedTry,

    TeamAFailedDropGoal,
    TeamAScoredDropGoal,

    TeamBFailedDropGoal,
    TeamBScoredDropGoal,

    TeamAScoredPenalty,
    TeamAMissedPenalty,

    TeamBScoredPenalty,
    TeamBMissedPenalty,

    TeamAPlayerAbducted,

    TeamBPlayerAbducted,

    TeamAPlayerRisksInjury,
    TeamAPlayerSeriousInjury,
    TeamAPlayerNonSeriousInjury,

    TeamBPlayerRisksInjury,
    TeamBPlayerSeriousInjury,
    TeamBPlayerNonSeriousInjury,

    TeamAPlayerYellowCard,
    TeamAPlayerReturningFromYellowCard,
    TeamAPlayerRedCard,

    TeamBPlayerYellowCard,
    TeamBPlayerReturningFromYellowCard,
    TeamBPlayerRedCard,

    TeamAGetsCheer,
    TeamBGetsCheer
}