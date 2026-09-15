using DailyRugby.Domain;

namespace DailyRugby.Application.Utilitaries;

public static class TacticHelpers
{
    public static bool IsStrongerThan(this Tactics tactic, Tactics other)
        => (tactic, other) switch
        {
            (Tactics.Physique, Tactics.Technique) => true,
            (Tactics.Technique, Tactics.Insight) => true,
            (Tactics.Insight, Tactics.Physique) => true,
            _ => false
        };
}