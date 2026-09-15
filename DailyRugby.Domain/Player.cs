namespace DailyRugby.Domain;

public record Player(Guid Id,
    int Number,
    int Insight,
    int Physique,
    int Technique,
    bool IsOnField,
    bool HasYellowCard = false,
    bool CanJoinField = true);