using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Web.AutoCompletes;
using Discord.Interactions;

namespace DailyRugby.Web.SlashCommands;

public class TeamSlashCommands(ITeamCrudService teamService)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("add-team", "Adds a team")]
    public async Task AddTeam(
        [Summary("championship", "The championship to add the team to")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId,
        [Summary("playerUsername", "The team's player")]
        string playerUsername,        
        int insight, int physique, int technique,
        [Summary("initialCoach", "The team's initial coach")]
        [Autocomplete(typeof(CoachAutoComplete))]
        string initialCoach)
    {
        await DeferAsync();

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid");
            return;
        }

        bool coachParsed = Enum.TryParse(initialCoach, true, out Coaches coach);
        if (!coachParsed)
        {
            await FollowupAsync("Invalid coach");
            return;
        }

        TeamAddRequest request = new(id,
            playerUsername,
            insight,
            physique,
            technique,
            coach);

        var addResult = await teamService.AddAsync(request);

        if (!addResult.IsSuccessful)
        {
            await FollowupAsync($"{addResult.Error}: {addResult.Message}");
            return;
        }

        await FollowupAsync($"Created successfully with the id {addResult.Item.Id}");
    }
}