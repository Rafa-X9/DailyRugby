using DailyRugby.Application.DTOs;
using DailyRugby.Shared;

namespace DailyRugby.Application.Interfaces;

public interface ITeamValidator
{
    Result Validate(TeamAddRequest team);
}