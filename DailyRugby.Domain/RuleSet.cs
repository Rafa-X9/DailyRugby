using DailyRugby.Shared;

namespace DailyRugby.Domain;

public abstract class RuleSet
{
    public abstract bool AllowCakes { get; }
    public abstract int StatsBudget { get; }
    public abstract bool HasMoraleBoost { get; }

    public abstract Result ValidateTeam(Team team);
}