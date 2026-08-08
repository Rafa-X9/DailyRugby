using DailyRugby.Domain;

namespace DailyRugby.Application.Interfaces;

public interface ITeamValidatorFactory
{
    ITeamValidator GetValidatorForSeason(Seasons season);
}