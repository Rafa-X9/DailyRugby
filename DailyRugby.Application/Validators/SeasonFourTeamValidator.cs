using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Shared;

namespace DailyRugby.Application.Validators;

public class SeasonFourTeamValidator : ITeamValidator
{
    public Result Validate(TeamAddRequest team)
    {
        int sum = team.Insight + team.Physique + team.Technique;
        if (sum != 100)
        {
            return Result.Failure("Stats must sum up to 100, " +
                $"instead they sum up to {sum}", Errors.Invalid);
        }

        return Result.Success();
    }
}