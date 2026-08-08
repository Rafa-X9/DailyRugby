using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;

namespace DailyRugby.Application.Validators;

public class TeamValidatorFactory : ITeamValidatorFactory
{
    public ITeamValidator GetValidatorForSeason(Seasons season)
    {
        return season switch
        {
            Seasons.Season1 => new SeasonOneTeamValidator(),
            _ => throw new NotImplementedException($"There are no validators for " +
                $"{season}'s teams yet")
        };
    }
}