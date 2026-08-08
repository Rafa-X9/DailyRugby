using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Web.AutoCompletes;
using Discord.Interactions;
using System.Text;

namespace DailyRugby.Web.SlashCommands;

public class ChampionshipSlashCommands(IChampionshipCrudService champService)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("add-championship", "Creates a championship")]
    public async Task AddChampionship(
        [Summary("name", "The name of the championship")] string name)
    {
        await DeferAsync();

        ChampionshipAddRequest request = new(name);

        var result = await champService.AddAsync(request);
        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }
        await FollowupAsync($"Success, the championship was created " +
            $"with the Id {result.Item.Id}");
    }

    [SlashCommand("see-championships", "See all created championships")]
    public async Task SeeChampionships()
    {
        await DeferAsync();
        var list = await champService.GetAllAsync();
        StringBuilder sb = new();
        sb.AppendLine("These are all championships registered:");
        foreach (var response in list)
        {
            sb.AppendLine($"- {response.Name}, State = {response.State}, Id = {response.Id}");
        }
        await FollowupAsync(sb.ToString());
    }

    [SlashCommand("delete-championship", "Deletes a championship")]
    public async Task DeleteChampionship(
        [Summary("id", "The Id of the championship you want to delete")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string id)
    {
        await DeferAsync();

        bool parsed = Guid.TryParse(id, out Guid guid);
        if (!parsed)
        {
            await FollowupAsync($"The id '{id}' isn't a valid Guid");
            return;
        }

        var result = await champService.DeleteAsync(guid);
        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }

        await FollowupAsync("Deleted successfully");
    }
}