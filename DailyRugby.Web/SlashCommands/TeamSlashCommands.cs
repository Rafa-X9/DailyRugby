using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Web.AutoCompletes;
using Discord.Interactions;
using System.Text;

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
        [Summary("country", "The team's country")]
        string country,
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
            country,
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

    [SlashCommand("see-teams", "Shows all teams in a championship")]
    public async Task SeeTeams(
        [Summary("championship", "The championship to see the teams from")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId)
    {
        await DeferAsync();

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid");
            return;
        }

        var allTeams = (await teamService.GetAllAsync(id)).OrderByDescending(team => team.Id);
        
        StringBuilder sb = new();
        sb.AppendLine("Teams in the championship:");
        foreach (var team in allTeams)
        {
            sb.AppendLine($"- {team.Country} by {team.PlayerUsername}; " +
                $"I = {team.Insight}, T = {team.Technique}, P = {team.Physique}, " +
                $"Coaches: {string.Join(", ", team.Coaches)}");
        }
        await FollowupAsync(sb.ToString());
    }

    [SlashCommand("delete-team", "Deletes a team from a championship")]
    public async Task DeleteTeam(
        [Summary("team", "The team to delete")]
        [Autocomplete(typeof(TeamAutoComplete))]
        string teamId)
    {
        await DeferAsync();

        bool idParsed = Guid.TryParse(teamId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid");
            return;
        }

        var result = await teamService.DeleteAsync(id);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }

        await FollowupAsync("Deleted successfully");
    }

    [SlashCommand("see-team-stats", "Shows the stats of a team")]
    public async Task SeeTeamStats(
        [Summary("team", "The team to see the stats from")]
        [Autocomplete(typeof(TeamAutoComplete))]
        string teamId)
    {
        await DeferAsync();

        bool idParsed = Guid.TryParse(teamId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid");
            return;
        }

        var result = await teamService.GetByIdAsync(id);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine($"{result.Item.Country} stats:");
        sb.AppendLine($"- {result.Item.WinCount} wins");
        sb.AppendLine($"- {result.Item.TieCount} ties");
        sb.AppendLine($"- {result.Item.LossCount} losses");
        sb.AppendLine($"- {result.Item.PointsScored} points scored");
        sb.AppendLine($"- {result.Item.PointsTaken} points suffered");
        sb.AppendLine($"- {result.Item.ScoredTriesCount} tries scored");

        await FollowupAsync(sb.ToString());
    }
}